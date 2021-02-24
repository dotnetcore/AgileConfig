import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button, message, Modal } from 'antd';
import React, { useState, useRef } from 'react';
import { NodeItem } from '../Nodes/data';
import { queryClients, reloadClientConfigs, clientOffline } from './service';
const { confirm } = Modal;

const handleClientReload = async (node:NodeItem, client:any)=>{
  const hide = message.loading('正在刷新');
  try {
    const result = await reloadClientConfigs(node.address, client.id);
    hide();
    const success = result.success;
    if (success) {
      message.success('刷新成功');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('刷新失败请重试！');
    return false;
  }
}


const handleClientOffline = async (node:NodeItem, client:any)=>{
  const hide = message.loading('正在断开');
  try {
    const result = await clientOffline(node.address, client.id);
    hide();
    const success = result.success;
    if (success) {
      message.success('断开成功');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('断开失败请重试！');
    return false;
  }
}

const clients:React.FC = () => {
  const actionRef = useRef<ActionType>();

  const columns: ProColumns[] = [
    {
      title: 'ID',
      dataIndex: 'id',
    },
    {
      title: '应用ID',
      dataIndex: 'appId',
    },
    {
      title: '操作',
      valueType: 'option',
      render: (text, record) => [
        <a
          onClick={() => {
            handleClientReload({address: 'xxx', status:1},record);
          }}
        >
          刷新配置
        </a>,
        <Button type="link" danger onClick={
         ()=>{
          const msg = `是否确定删除客户端【${record.id}】?`;
          confirm({
            icon: <ExclamationCircleOutlined />,
            content: msg,
            async onOk() {
              console.log('disconnect client ' + record.id);
              const success = await handleClientOffline({address: 'xxx', status:1},record);
              if (success) {
                actionRef.current?.reload();
              }
            },
            onCancel() {
            },
          });
          }}>
          断开
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
        columns = {columns}
        search={false}
        request = { (params, sorter, filter) => queryClients() }
      />
    </PageContainer>
  );
}
export default clients;
