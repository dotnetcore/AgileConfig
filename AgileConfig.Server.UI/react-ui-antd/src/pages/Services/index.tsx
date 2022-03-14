import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import React, {  useRef } from 'react';
import { queryService } from './service';


const services: React.FC = () => {
  const actionRef = useRef<ActionType>();
  const columns: ProColumns[] = [
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
      title: '心跳模式',
      dataIndex: 'heartBeatMode',
      hideInSearch: true,
    },
    {
      title: '检测URL',
      dataIndex: 'checkUrl',
      hideInSearch: true,
      ellipsis: true,
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
