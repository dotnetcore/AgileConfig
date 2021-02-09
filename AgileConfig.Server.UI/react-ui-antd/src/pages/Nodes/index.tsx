import { PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button } from 'antd';
import React, { useState, useRef } from 'react';
import { queryNodes } from './service';

const nodeList:React.FC = () => {
  const columns: ProColumns[] = [
    {
      title: '节点地址',
      dataIndex: 'address',
    },
    {
      title: '备注',
      dataIndex: 'remark',
    },
    {
      title: '最后响应时间',
      dataIndex: 'lastEchoTime',
      valueType: 'dateTime'
    },
    {
      title: '状态',
      dataIndex: 'status',
      valueEnum: {
        1:{
          text: '在线'
        },
        0: {
          text: '离线'
        }
      }
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
          编辑
        </a>,
        <a href={record.url} target="_blank" rel="noopener noreferrer" key="view">
          查看
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
        request = { (params, sorter, filter) => queryNodes() }
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary">
            新建
          </Button>
        ]}
      />
    </PageContainer>
  );
}
export default nodeList;
