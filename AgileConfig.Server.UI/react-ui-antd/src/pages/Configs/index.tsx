import { PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ProColumns } from '@ant-design/pro-table';
import { Button } from 'antd';
import React, { useState, useRef } from 'react';
import { FormattedMessage } from 'umi';
import { TableListItem } from '../TableList/data';
import { queryConfigs } from './service';

const configs:React.FC = () => {
  const columns: ProColumns<any>[] = [
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
      dataIndex: 'onlineStatus',
      hideInSearch: true,
      valueEnum: {
        0: {
          text:'待上线',
          status: 'Default'
        },
        1: {
          text:'已上线',
          status: 'Success'
        }
      }
    },
    {
      title: '操作',
      valueType: 'option',
      render: (text, record, _, action) => [
        <a>下线</a>,
        <a
          key="editable"
          onClick={() => {
            action.startEditable?.(record.id);
          }}
        >
          编辑
        </a>,
        <a>版本历史</a>,
        <Button type="link" danger>
          删除
        </Button>
      ]
    },
  ];
  return (
    <PageContainer>
      <ProTable<any>     
        rowKey="id"
        options={
          false
        }
        columns = {columns}
        search={{
          labelWidth: 120,
        }}
        request = { (params, sorter, filter) => queryConfigs() }
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary">
            新建
          </Button>,
          <Button key="button" type="primary">
          上线选中的配置
        </Button>,
          <Button key="button" type="primary">
            从json文件导入
          </Button>
        ]}
      />
    </PageContainer>
  );
}
export default configs;
