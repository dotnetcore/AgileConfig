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
        <Button type="link" danger>
          断开
        </Button>
      ]
    }
  ];
  return (
    <PageContainer>
      <ProTable      
      options={
        false
      }                                                                              
        rowKey="id"
        columns = {columns}
        search={false}
        request = { (params, sorter, filter) => queryClients() }
      />
    </PageContainer>
  );
}
export default clients;
