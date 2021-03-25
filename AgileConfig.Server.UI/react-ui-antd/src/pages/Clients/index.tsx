import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button, message, Modal } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { queryApps } from '../Apps/service';
import { queryNodes } from '../Nodes/service';
import { queryClients, reloadClientConfigs, clientOffline } from './service';
const { confirm } = Modal;

const handleClientReload = async (client:any)=>{
  const hide = message.loading('正在刷新');
  try {
    const result = await reloadClientConfigs(client.address, client.id);
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


const handleClientOffline = async (client:any)=>{
  const hide = message.loading('正在断开');
  try {
    const result = await clientOffline(client.address, client.id);
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
  const [appEnums, setAppEnums] = useState<any>();
  const getNodesForSelect = async () =>
  {
    const result = await queryNodes()
    const arr:any[] = [];
    result.data.forEach((x:{address:string})=>{
       arr.push({
         value: x.address
       });
    });

    return arr;
  }
  const getAppEnums = async () =>
  {
    const result = await queryApps({})
    const obj = {};
    result.data?.forEach(x=>{
      if(x) {
        obj[x.id] = {
          text: x.name
        }
      }
    });

    return obj;
  }
  useEffect(()=>{
    getAppEnums().then(x=> {
      console.log('app enums ', x);
      setAppEnums({...x});
    });
  }, []);
  const columns: ProColumns[] = [
    {
      title: 'ID',
      dataIndex: 'id',
      hideInSearch: true,
    },
    {
      title: '节点',
      dataIndex: 'address',
      valueType: 'select',
      request: getNodesForSelect
    },
    {
      title: '应用ID',
      dataIndex: 'appId',
      hideInSearch: true,
    },
    {
      title: '应用名称',
      dataIndex: 'appId',
      valueType: 'select',
      valueEnum: appEnums,
      hideInSearch: true,
    },
    
    {
      title: '操作',
      valueType: 'option',
      render: (text, record) => [
        <a
          onClick={() => {
            handleClientReload(record);
          }}
        >
          刷新配置
        </a>,
        <Button type="link" danger onClick={
         ()=>{
          const msg = `是否确定断开与客户端【${record.id}】的连接?`;
          confirm({
            icon: <ExclamationCircleOutlined />,
            content: msg,
            async onOk() {
              console.log('disconnect client ' + record.id);
              const success = await handleClientOffline(record);
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
    <PageContainer header={{ title:'连接的客户端' }}>
      <ProTable     
      search={{
        labelWidth: 'auto',
      }}
      actionRef={actionRef} 
      options={
        false
      }                                                                              
        rowKey="id"
        columns = {columns}
        request = { (params, sorter, filter) => queryClients(params) }
      />
    </PageContainer>
  );
}
export default clients;
