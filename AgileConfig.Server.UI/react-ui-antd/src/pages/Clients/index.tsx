import { getIntl, getLocale } from '@/.umi/plugin-locale/localeExports';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, message, Modal } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { queryApps } from '../Apps/service';
import { queryNodes } from '../Nodes/service';
import { queryClients, reloadClientConfigs, clientOffline } from './service';
const { confirm } = Modal;

const handleClientReload = async (client:any)=>{
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id: 'refreshing'}));
  try {
    const result = await reloadClientConfigs(client.address, client.id);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id: 'refresh_success'}));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id: 'refresh_fail'}));
    return false;
  }
}


const handleClientOffline = async (client:any)=>{
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id: 'disconnecting'}));
  try {
    const result = await clientOffline(client.address, client.id);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id: 'disconnect_success'}));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id: 'disconnect_fail'}));
    return false;
  }
}

const clients:React.FC = () => {
  const actionRef = useRef<ActionType>();
  const [appEnums, setAppEnums] = useState<any>();
  const intl = useIntl();

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
      title: intl.formatMessage({
        id: 'pages.client.table.cols.id'
      }),
      dataIndex: 'id',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.node'
      }),
      dataIndex: 'address',
      valueType: 'select',
      request: getNodesForSelect
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.appid'
      }),
      dataIndex: 'appId',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.appname'
      }),
      dataIndex: 'appId',
      valueType: 'select',
      valueEnum: appEnums,
      hideInSearch: true,
    },
    
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.action'
      }),
      valueType: 'option',
      render: (text, record) => [
        <a
          onClick={() => {
            handleClientReload(record);
          }}
        >
          {
            intl.formatMessage({
              id: 'pages.client.table.cols.action.refresh'
            })
          }
        </a>,
        <Button type="link" danger onClick={
         ()=>{
          const msg = intl.formatMessage({
                        id: 'pages.client.disconnect_message'
                      }) + `【${record.id}】`;
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
           {
             intl.formatMessage({
              id: 'pages.client.table.cols.action.disconnect'
            })
           }
        </Button>
      ]
    }
  ];
  return (
    <PageContainer header={{ title:intl.formatMessage({id:'pages.client.header.title'}) }}>
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
