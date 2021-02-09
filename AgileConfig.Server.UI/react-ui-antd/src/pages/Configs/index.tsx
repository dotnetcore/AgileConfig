import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ProColumns } from '@ant-design/pro-table';
import React, { useState, useRef } from 'react';
import { FormattedMessage } from 'umi';
import { TableListItem } from '../TableList/data';
import { queryConfigs } from './service';

const configs:React.FC = () => {
  const columns: ProColumns<TableListItem>[] = [
    {
      title: '组',
      dataIndex: 'appId',
    },
    {
      title: '键',
      dataIndex: 'logType',
    },
    {
      title: '值',
      dataIndex: 'logTime',
      valueType: 'dateTime',
      hideInSearch: true,
    },
    {
      title: '描述',
      dataIndex: 'logTime',
      hideInSearch: true,
    },
    {
      title: '创建时间',
      dataIndex: 'logTxt',
      hideInSearch: true
    },
    {
      title: '修改时间',
      dataIndex: 'logTxt',
      hideInSearch: true
    },
    {
      title: '状态',
      dataIndex: 'logTxt',
      hideInSearch: true
    },
    {
      title: '操作',
      dataIndex: 'logTxt',
      hideInSearch: true
    },
  ];
  return (
    <PageContainer>
      <ProTable<TableListItem>                                                                                    
        rowKey="id"
        columns = {columns}
        search={{
          labelWidth: 120,
        }}
        request = { (params, sorter, filter) => queryConfigs() }
      />
    </PageContainer>
  );
}
export default configs;
