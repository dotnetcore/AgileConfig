import { getIntl, getLocale } from '@/.umi/plugin-locale/localeExports';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, message, Modal } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { useIntl } from 'react-intl';
import { queryApps } from '../Apps/service';
import { queryNodes } from '../Nodes/service';
import { queryService, reloadClientConfigs, clientOffline } from './service';
const { confirm } = Modal;

const handleClientReload = async (client: any) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({ id: 'refreshing' }));
  try {
    const result = await reloadClientConfigs(client.address, client.id);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({ id: 'refresh_success' }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({ id: 'refresh_fail' }));
    return false;
  }
}


const handleClientOffline = async (client: any) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({ id: 'disconnecting' }));
  try {
    const result = await clientOffline(client.address, client.id);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({ id: 'disconnect_success' }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({ id: 'disconnect_fail' }));
    return false;
  }
}

const services: React.FC = () => {
  const actionRef = useRef<ActionType>();
  const [appEnums, setAppEnums] = useState<any>();
  const intl = useIntl();

  const getNodesForSelect = async () => {
    const result = await queryNodes()
    const arr: any[] = [];
    result.data.forEach((x: { address: string }) => {
      arr.push({
        value: x.address,
        label: x.address,
      });
    });

    return arr;
  }
  const getAppEnums = async () => {
    const result = await queryApps({})
    const obj = {};
    result.data?.forEach(x => {
      if (x) {
        obj[x.id] = {
          text: x.name
        }
      }
    });

    return obj;
  }
  useEffect(() => {
    getAppEnums().then(x => {
      console.log('app enums ', x);
      setAppEnums({ ...x });
    });
  }, []);
  const columns: ProColumns[] = [
    {
      title:'唯一ID',
      dataIndex: 'id',
      hideInSearch: true,
    },
    {
      title: '服务ID',
      dataIndex: 'serviceId',
    },
    {
      title: '服务名',
      dataIndex: 'serviceName',
      sorter: true,
    },
    {
      title: 'IP',
      dataIndex: 'ip',
      hideInSearch: true,
    },
    {
      title: '端口',
      dataIndex: 'port',
      hideInSearch: true,
    },
    {
      title: '元数据',
      dataIndex: 'metaData',
      hideInSearch: true,
    },
    {
      title: '注册时间',
      dataIndex: 'registerTime',
      hideInSearch: true,
      valueType: 'dateTime',
      sorter: true,
    },
    {
      title: '最后响应时间',
      dataIndex: 'lastHeartBeat',
      hideInSearch: true,
      valueType: 'dateTime',
    },
    {
      title: '状态',
      dataIndex: 'alive',
      valueEnum: {
        0: {
          text: '离线',
          status: 'Default'
        },
        1: {
          text: '在线',
          status: 'Processing'
        }
      },
      width: 120
    }
  ];
  return (
    <PageContainer>
      <ProTable
        search={{
          labelWidth: 'auto',
        }}
        actionRef={actionRef}
        options={
          false
        }
        rowKey="id"
        columns={columns}
        request={(params, sorter, filter) => {
          let sortField = 'registerTime';
          let ascOrDesc = 'descend';
          for (const key in sorter) {
            sortField = key;
            const val = sorter[key];
            if (val) {
              ascOrDesc = val;
            }
          }
          console.log(sortField, ascOrDesc);
          return queryService({ sortField, ascOrDesc, ...params })
        }}
      />
    </PageContainer>
  );
}
export default services;
