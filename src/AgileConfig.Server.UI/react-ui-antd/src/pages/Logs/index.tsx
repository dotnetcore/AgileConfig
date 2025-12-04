import { PageContainer } from '@ant-design/pro-layout';
import { RequireFunction } from '@/utils/permission';
import ProTable, { ProColumns } from '@ant-design/pro-table';
import React, { useState, useEffect } from 'react';
import { useIntl } from 'umi';
import { queryLogs } from './service';
import { queryApps } from '../Apps/service';

const logs:React.FC = () =>  {
  const intl = useIntl();
  const [appEnums, setAppEnums] = useState<any>();
  const getAppEnums = async () => {
    const result = await queryApps({ sortField: 'createTime', ascOrDesc: 'descend', tableGrouped: false });
    const obj: Record<string, { text: string }> = {};
    result.data.forEach((x: any) => {
      if (x.id && x.name) {
        obj[x.id] = { text: x.name };
      }
    });
    return obj;
  };
  useEffect(()=>{
    getAppEnums().then(x=> {
      console.log('app enums ', x);
      setAppEnums({...x});
    });
  }, []);
 
  const columns: ProColumns[] = [
    {
      title: intl.formatMessage({id: 'pages.logs.table.appname'}),
      dataIndex: 'appId',
      valueType: 'select',
      valueEnum: appEnums,
    },
    {
      title: intl.formatMessage({id: 'pages.logs.table.type'}),
      dataIndex: 'logType',
      valueType: 'select',
      valueEnum: {
        0: {
          text: intl.formatMessage({id: 'pages.logs.table.type.0'}),
        },
        1: {
          text:  intl.formatMessage({id: 'pages.logs.table.type.1'}),
        }
      },
    },
    {
      title: intl.formatMessage({id: 'pages.logs.table.time'}),
      dataIndex: 'logTime',
      valueType: 'dateTime',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({id: 'pages.logs.table.time'}),
      dataIndex: 'logTime',
      valueType: 'dateRange',
      hideInTable: true,
      search: {
        transform: (value) => {
          return {
            startTime: value[0],
            endTime: value[1],
          };
        },
      }
    },
    {
      title: intl.formatMessage({id: 'pages.logs.table.content'}),
      dataIndex: 'logText',
      hideInSearch: true
    },
  ];
  return (
    <PageContainer>
      <RequireFunction fn="LOG_READ" fallback={<div>{intl.formatMessage({ id: 'pages.role.permissions.LOG_READ', defaultMessage: 'No log permission' })}</div>}>
        <ProTable
          options={false}
          search={{ labelWidth: 'auto' }}
          rowKey="id"
          columns={columns}
          request={(params) => queryLogs({ ...params })}
        />
      </RequireFunction>
    </PageContainer>
  );
}
export default logs;
