import { PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button } from 'antd';
import React, { useState, useRef } from 'react';
import { queryClients } from './service';

const clients:React.FC = () => {
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
      render: (text, record, _, action) => [
        <a
          key="editable"
          onClick={() => {
            action.startEditable?.(record.id);
          }}
        >
          刷新配置
        </a>,
        <a href={record.url} target="_blank" rel="noopener noreferrer" key="view">
          断开
        </a>
      ]
    }
  ];
  return (
    <PageContainer>
      <ProTable                                                                                    
        rowKey="id"
        columns = {columns}
        search={false}
        request = { (params, sorter, filter) => queryClients() }
      />
    </PageContainer>
  );
}
export default clients;
