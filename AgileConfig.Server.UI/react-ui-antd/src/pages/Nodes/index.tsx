import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { ModalForm, ProFormText, ProFormTextArea } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button, FormInstance, message,Modal } from 'antd';
import React, { useState, useRef } from 'react';
import { NodeItem } from './data';
import { queryNodes, addNode, delNode } from './service';

const { confirm } = Modal;
const handleAdd = async (fields: NodeItem) => {
  const hide = message.loading('正在添加');
  try {
    const result = await addNode({ ...fields });
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
const handleDel = async (fields: NodeItem) => {
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

const nodeList:React.FC = () => {
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();

  const [createModalVisible, handleModalVisible] = useState<boolean>(false);
  const columns: ProColumns[] = [
    {
      title: '节点地址',
      dataIndex: 'address',
    },
    {
      title: '备注',
      dataIndex: 'remark',
    },
    {
      title: '最后响应时间',
      dataIndex: 'lastEchoTime',
      valueType: 'dateTime'
    },
    {
      title: '状态',
      dataIndex: 'status',
      valueEnum: {
        1:{
          text: '在线',
          status: 'Processing'
        },
        0: {
          text: '离线',
          status: 'Default'
        }
      }
    },
    {
      title: '操作',
      valueType: 'option',
      render: (text, record, _, action) => [
        <a>
          刷新所有客户端的配置
        </a>,
        <Button  type="link" danger
          onClick={ ()=> {
            const msg = `是否确定删除节点【${record.address}】?`;
            confirm({
              icon: <ExclamationCircleOutlined />,
              content: <div>
                          <div>{msg}</div>
                          <br></br>
                          <div>删除节点并不会让其真正的下线，只是脱离控制台的管理。所有连接至此节点的客户端都会继续正常工作。</div>
                        </div>,
              async onOk() {
                console.log('delete node ' + record.address);
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
        </Button >
      ]
    }
  ];
  return (
    <PageContainer>
      <ProTable     
      options={
        false
      }
        rowKey="address"
        actionRef={actionRef}
        columns = {columns}
        search={false}
        request = { (params, sorter, filter) => queryNodes() }
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary"
          onClick={ ()=>{ handleModalVisible(true) } }
          >
            添加
          </Button>
        ]}
      />
      <ModalForm 
        formRef={addFormRef}
        title="添加节点" 
        width="400px"
        visible={createModalVisible}
        onVisibleChange={handleModalVisible}
        onFinish={
          async (value) => {
            const success = await handleAdd(value as NodeItem);
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
          label="节点地址"
          width="md"
          name="address"
          tooltip="请输入节点的IP跟PORT，如 http://192.168.0.120:5000"
        />
        <ProFormTextArea
          label="备注"
          width="md"
          name="remark"
        />
      </ModalForm>
    </PageContainer>
  );
}
export default nodeList;
