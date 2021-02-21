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
      valueType: 'select',
      valueEnum: {
        '': { text: '全部', status: '' },
        test_app: {
          text: '测试应用',
        }
      },
    },
    {
      title: '类型',
      dataIndex: 'logType',
      valueType: 'select',
      valueEnum: {
        '': { text: '全部', status: '' },
        0: {
          text: '普通',
        },
        1: {
          text: '警告',
        }
      },
    },
    {
      title: '时间',
      dataIndex: 'logTime',
      valueType: 'dateTime',
      hideInSearch: true,
    },
    {
      title: '时间',
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
      title: '内容',
      dataIndex: 'logTxt',
      hideInSearch: true
    },
  ];
  return (
    <PageContainer>
      <ProTable<TableListItem>       
      options={
        false
      }                                                                             
        rowKey="id"
        columns = {columns}
        search={{
          labelWidth: 120,
        }}
        request = { (params, sorter, filter) => queryLogs({ ...params}) }
      />
    </PageContainer>
  );
}
export default logs;
