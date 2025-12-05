// migrated to RequireFunction for permission gating
import { RequireFunction } from '@/utils/permission';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, message, Modal } from 'antd';
import React, { useRef } from 'react';
import { useIntl } from 'umi';
import { queryNodes } from '../Nodes/service';
import { queryClients, reloadClientConfigs, clientOffline } from './service';
const { confirm } = Modal;

const clients: React.FC = () => {
  const intl = useIntl();
  const actionRef = useRef<ActionType>();

  const handleClientReload = async (client: any) => {
    const hide = message.loading(intl.formatMessage({ id: 'refreshing' }));
    try {
      const result = await reloadClientConfigs(client.address, client.id);
      hide();
      const success = result.success;
      if (success) {
        message.success(intl.formatMessage({ id: 'refresh_success' }));
      } else {
        message.error(result.message);
      }
      return success;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({ id: 'refresh_fail' }));
      return false;
    }
  };

  const handleClientOffline = async (client: any) => {
    const hide = message.loading(intl.formatMessage({ id: 'disconnecting' }));
    try {
      const result = await clientOffline(client.address, client.id);
      hide();
      const success = result.success;
      if (success) {
        message.success(intl.formatMessage({ id: 'disconnect_success' }));
      } else {
        message.error(result.message);
      }
      return success;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({ id: 'disconnect_fail' }));
      return false;
    }
  };

  const getNodesForSelect = async () => {
    const result = await queryNodes();
    const arr: any[] = [];
    result.data.forEach((x: { address: string }) => {
      arr.push({
        value: x.address,
        label: x.address,
      });
    });

    return arr;
  };

  const columns: ProColumns[] = [
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.id',
      }),
      dataIndex: 'id',
      hideInSearch: true,
      ellipsis: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.node',
      }),
      dataIndex: 'address',
      valueType: 'select',
      request: getNodesForSelect,
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.env',
      }),
      dataIndex: 'env',
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.appid',
      }),
      dataIndex: 'appId',
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.ip',
      }),
      dataIndex: 'ip',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.name',
      }),
      dataIndex: 'name',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.tag',
      }),
      dataIndex: 'tag',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.client.table.cols.lastRefreshTime',
      }),
      dataIndex: 'lastRefreshTime',
      hideInSearch: true,
      valueType: 'dateTime',
      tooltip: intl.formatMessage({
        id: 'pages.client.table.cols.lastRefreshTime.tooltip',
      }),
    },
    {
      title: intl.formatMessage({ id: 'pages.client.table.cols.action' }),
      valueType: 'option',
      render: (text, record) => [
        <RequireFunction fn="CLIENT_REFRESH" key="refresh" fallback={null}>
          <a
            onClick={() => {
              handleClientReload(record);
            }}
          >
            {intl.formatMessage({ id: 'pages.client.table.cols.action.refresh' })}
          </a>
        </RequireFunction>,
        <RequireFunction fn="CLIENT_DISCONNECT" key="disconnect" fallback={null}>
          <Button
            type="link"
            danger
            onClick={() => {
              const msg = intl.formatMessage({ id: 'pages.client.disconnect_message' }) + `【${record.id}】`;
              confirm({
                icon: <ExclamationCircleOutlined />,
                content: msg,
                async onOk() {
                  const success = await handleClientOffline(record);
                  if (success) {
                    actionRef.current?.reload();
                  }
                },
              });
            }}
          >
            {intl.formatMessage({ id: 'pages.client.table.cols.action.disconnect' })}
          </Button>
        </RequireFunction>,
      ],
    },
  ];
  return (
    <PageContainer header={{ title: intl.formatMessage({ id: 'pages.client.header.title' }) }}>
      <ProTable
        search={{
          labelWidth: 'auto',
        }}
        actionRef={actionRef}
        options={false}
        rowKey="id"
        columns={columns}
        request={(params, sorter, filter) => queryClients(params)}
      />
    </PageContainer>
  );
};
export default clients;
