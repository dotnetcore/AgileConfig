import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ProColumns } from '@ant-design/pro-table';
import React, { useState, useRef } from 'react';
import { FormattedMessage } from 'umi';
import { TableListItem } from '../TableList/data';
import { queryLogs } from './service';

const logs:React.FC = () => {
  const columns: ProColumns<TableListItem>[] = [
    {
      title: '应用',
      dataIndex: 'appId',
    },
    {
      title: '类型',
      dataIndex: 'logType',
    },
    {
      title: '时间',
      dataIndex: 'logTime',
      valueType: 'dateRange'
    },
    {
      title: '内容',
      dataIndex: 'logTxt',
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
        request = { (params, sorter, filter) => queryLogs({ ...params}) }
        rowSelection={{
          onChange: (_, selectedRows) => {
          },
        }}
      />
    </PageContainer>
  );
}
export default logs;
