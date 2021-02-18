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
      dataIndex: 'group',
    },
    {
      title: '键',
      dataIndex: 'key',
    },
    {
      title: '值',
      dataIndex: 'value',
    },
    {
      title: '描述',
      dataIndex: 'description',
      hideInSearch: true,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      hideInSearch: true,
      valueType: 'dateTime'
    },
    {
      title: '修改时间',
      dataIndex: 'updateTime',
      hideInSearch: true,
      valueType: 'dateTime'
    },
    {
      title: '状态',
      dataIndex: 'status',
      hideInSearch: true,
      valueEnum: {
        0: '未上线',
        1: '已上线'
      }
    },
    {
      title: '操作',
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
