import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { ModalForm,  ProFormDependency, ProFormSelect, ProFormSwitch, ProFormText } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, FormInstance, message, Modal, Switch } from 'antd';
import React, { useState, useRef } from 'react';
import UpdateForm from './comps/updateForm';
import { AppListItem } from './data';
import { addApp, editApp, delApp, queryApps, inheritancedApps } from './service';

const { confirm } = Modal;
const handleAdd = async (fields: AppListItem) => {
  const hide = message.loading('正在保存');
  try {
    const result = await addApp({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('新建成功');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('新建失败请重试！');
    return false;
  }
};
const handleEdit = async (app: AppListItem) => {
  const hide = message.loading('正在保存');
  try {
    const result = await editApp({ ...app });
    hide();
    const success = result.success;
    if (success) {
      message.success('保存成功');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('保存失败请重试！');
    return false;
  }
};
const handleDel = async (fields: AppListItem) => {
  const hide = message.loading('正在删除');
  try {
    const result = await delApp({ ...fields });
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

const appList: React.FC = () => {
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();

  const [createModalVisible, handleModalVisible] = useState<boolean>(false);
  const [updateModalVisible, handleUpdateModalVisible] = useState<boolean>(false);
  const [currentRow, setCurrentRow] = useState<AppListItem>();

  const columns: ProColumns<AppListItem>[] = [
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
      render: (dom, entity) => {
        return <Switch checked={entity.inheritanced} size="small" />
      },
      hideInSearch: true
    },
    {
      title: '启用',
      dataIndex: 'enabled',
      render: (dom, entity) => {
        return <Switch checked={entity.enabled} size="small" />
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
            handleUpdateModalVisible(true);
            setCurrentRow(record);
            console.log('select app ', record);
            console.log('current app ', currentRow);
          }}
        >
          编辑
        </a>,
        <Button type="link" danger
          onClick={() => {
            const msg = `是否确定删除应用【${record.name}】?`;
            confirm({
              icon: <ExclamationCircleOutlined />,
              content: msg,
              async onOk() {
                console.log('delete app ' + record.name);
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
        actionRef={actionRef}
        options={
          false
        }
        rowKey="id"
        columns={columns}
        request={(params, sorter, filter) => queryApps(params)}
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary" onClick={() => { handleModalVisible(true) }}>
            新建
          </Button>
        ]}
      />
      <ModalForm
        formRef={addFormRef}
        title="新建应用"
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
          name="secret"
        />
        <ProFormSwitch tooltip="公共应用可以被其他应用关联" label="公共应用" name="inheritanced"></ProFormSwitch>
        <ProFormDependency name={
          ["inheritanced"]
        }>
          {
            (e) => {
              return !e.inheritanced ?
                <ProFormSelect
                  tooltip="关联后可以读取公共应用的配置项"
                  label="关联应用"
                  name="inheritancedApps"
                  mode="multiple"
                  request={async () => {
                    const result = await inheritancedApps();
                    return result.data.map( (x: { name: string, id: string })=> {
                      return { lable:x.name, value:x.id};
                    });
                  }}
                ></ProFormSelect> : null
            }
          }
        </ProFormDependency>
        <ProFormSwitch label="启用" name="enabled" initialValue="true">
        </ProFormSwitch>
      </ModalForm>
      {
        updateModalVisible &&
        <UpdateForm
          value={{ ...currentRow }}
          setValue={setCurrentRow}
          updateModalVisible={updateModalVisible}
          onCancel={
            () => {
              setCurrentRow(undefined);
              handleUpdateModalVisible(false);
              console.log('set currentrow undefined');
            }
          }
          onSubmit={
            async (value) => {
              setCurrentRow(undefined);
              const success = await handleEdit(value);
              if (success) {
                handleUpdateModalVisible(false);
                if (actionRef.current) {
                  actionRef.current.reload();
                }
              }
              addFormRef.current?.resetFields();
            }
          }></UpdateForm>
      }
    </PageContainer>
  );
}
export default appList;
