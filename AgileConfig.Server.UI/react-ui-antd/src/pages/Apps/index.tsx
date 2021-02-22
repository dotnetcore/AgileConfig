import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { ModalForm, ProFormCheckbox, ProFormSelect, ProFormSwitch, ProFormText } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button, FormInstance, message, Modal, Switch } from 'antd';
import React, { useState, useRef } from 'react';
import UpdateForm from './comps/updateForm';
import { addApp, delNode, queryApps } from './service';

const { confirm } = Modal;
const handleAdd = async (fields: any) => {
  const hide = message.loading('正在添加');
  try {
    const result = await addApp({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('添加成功');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('添加失败请重试！');
    return false;
  }
};
const handleDel = async (fields:any) => {
  const hide = message.loading('正在删除');
  try {
    const result = await delNode({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('删除成功');
    } else {
      message.error('删除失败请重试！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('删除失败请重试！');
    return false;
  }
};

const appList:React.FC = () => {
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();

  const [createModalVisible, handleModalVisible] = useState<boolean>(false);
  const [updateModalVisible, handleUpdateModalVisible] = useState<boolean>(false);

  const columns: ProColumns[] = [
    {
      title: '名称',
      dataIndex: 'name',
    },
    {
      title: '应用ID',
      dataIndex: 'id',
    },
    {
      title: '密钥',
      dataIndex: 'secret',
      valueType: 'password',
      hideInSearch: true
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      valueType: 'dateTime',
      hideInSearch: true
    },
    {
        title: '修改时间',
        dataIndex: 'updateTime',
        valueType: 'dateTime',
        hideInSearch: true
      },
      {
        title: '公共',
        dataIndex: 'inheritanced',
        render: (dom, entry) =>{
          return  <Switch checked={entry.inheritanced} size="small"/>
        },
        hideInSearch: true
      },
      {
        title: '启用',
        dataIndex: 'enabled',
        render: (dom, entry) =>{
          return  <Switch checked={entry.enabled} size="small"/>
        },
        hideInSearch: true
      },
    {
      title: '操作',
      valueType: 'option',
      render: (text, record, _, action) => [
        <a
          key="editable"
          onClick={() => {
            handleUpdateModalVisible(true)
          }}
        >
          编辑
        </a>,
        <Button type="link" danger
        onClick={ ()=> {
          const msg = `是否确定删除应用【${record.name}】?`;
          confirm({
            icon: <ExclamationCircleOutlined />,
            content:  msg,
            async onOk() {
              console.log('delete app ' + record.address);
              const success = await handleDel(record);
              if (success) {
                actionRef.current?.reload();
              }
            },
            onCancel() {
              console.log('Cancel');
            },
          });
        }}
        >
          删除
        </Button>
      ]
    }
  ];
  return (
    <PageContainer>
      <ProTable    
      actionRef = {actionRef}  
      options={
        false
      }                                                                              
        rowKey="id"
        columns = {columns}
        request = { (params, sorter, filter) => queryApps() }
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary" onClick={ ()=>{ handleModalVisible(true) } }>
            新建
          </Button>
        ]}
      />
            <ModalForm 
        formRef={addFormRef}
        title="添加应用" 
        visible={createModalVisible}
        onVisibleChange={handleModalVisible}
        onFinish={
          async (value) => {
            const success = await handleAdd(value);
            if (success) {
              handleModalVisible(false);
              if (actionRef.current) {
                actionRef.current.reload();
              }
            }
            addFormRef.current?.resetFields();
          }
        }
      >
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label="名称"
          name="name"
        />
     <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label="ID"
          name="id"
        />
        <ProFormText.Password
          rules={[
            {
            },
          ]}
          label="密钥"
          name="password"
        />
        <ProFormSwitch label="公共应用" name="inheritanced"></ProFormSwitch>
        <ProFormSelect label="关联应用" name="inheritancedApps"
        mode="multiple" 
        request={async () => [
          { label: 'app1', value: '1' },
          { label: 'app2', value: '2' },
          { label: 'app3', value: '3' },
          { label: 'app4', value: '4' },
        ]}
        ></ProFormSelect>
        <ProFormSwitch label="启用" name="enabled"></ProFormSwitch>
      </ModalForm>
      <UpdateForm
      handleModalVisible = {handleUpdateModalVisible}
      updateModalVisible = {updateModalVisible}
      onSubmit={
        async (value) => {
          const success = await handleAdd(value);
          if (success) {
            handleUpdateModalVisible(false);
            if (actionRef.current) {
              actionRef.current.reload();
            }
          }
          addFormRef.current?.resetFields();
        }
      }></UpdateForm>
    </PageContainer>
  );
}
export default appList;
