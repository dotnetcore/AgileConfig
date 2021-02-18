import { PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button } from 'antd';
import React, { useState, useRef } from 'react';
import { queryApps } from './service';

const appList:React.FC = () => {
  const columns: ProColumns[] = [
    {
      title: '名称',
      dataIndex: 'name',
    },
    {
      title: '应用ID',
      dataIndex: 'id',
    },
    {
      title: '密钥',
      dataIndex: 'secret',
      valueType: 'password'
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      valueType: 'dateTime'
    },
    {
        title: '修改时间',
        dataIndex: 'updateTime',
        valueType: 'dateTime'
      },
      {
        title: '可被继承',
        dataIndex: 'inheritanced',
        valueEnum: {
          true:{
            text: '是'
          },
          false: {
            text: '否'
          }
        }
      },
      {
        title: '启用',
        dataIndex: 'enabled',
        valueEnum: {
          true:{
            text: '是'
          },
          false: {
            text: '否'
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
          停用
        </a>,
        <a href={record.url} target="_blank" rel="noopener noreferrer" key="view">
          删除
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
        request = { (params, sorter, filter) => queryApps() }
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary">
            新建
          </Button>
        ]}
      />
    </PageContainer>
  );
}
export default appList;
