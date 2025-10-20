import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, message, Modal, Space, Tag } from 'antd';
import { ModalForm, ProFormSelect, ProFormText } from '@ant-design/pro-form';
import React, { useEffect, useRef, useState } from 'react';
import { useIntl } from 'umi';
import type { RoleFormValues, RoleItem } from './data';
import { createRole, deleteRole, fetchSupportedRolePermissions, queryRoles, updateRole } from '@/services/role';

const { confirm } = Modal;

const RolePage: React.FC = () => {
  const actionRef = useRef<ActionType>();
  const intl = useIntl();
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [updateModalVisible, setUpdateModalVisible] = useState(false);
  const [currentRole, setCurrentRole] = useState<RoleItem | undefined>();
  const [supportedPermissions, setSupportedPermissions] = useState<string[]>([]);

  useEffect(() => {
    loadPermissions();
  }, []);

  const loadPermissions = async () => {
    try {
      const response = await fetchSupportedRolePermissions();
      if (response?.success && Array.isArray(response.data)) {
        setSupportedPermissions(response.data);
      } else {
        message.error(intl.formatMessage({ id: 'pages.role.permissions.load_failed', defaultMessage: 'Failed to load permissions' }));
      }
    } catch (error) {
      message.error(intl.formatMessage({ id: 'pages.role.permissions.load_failed', defaultMessage: 'Failed to load permissions' }));
    }
  };

  const permissionOptions = supportedPermissions.map((item) => ({
    value: item,
    label: item,
  }));

  const handleCreate = async (values: RoleFormValues) => {
    const hide = message.loading(intl.formatMessage({ id: 'saving', defaultMessage: 'Saving...' }));
    try {
      const response = await createRole(values);
      hide();
      if (response?.success) {
        message.success(intl.formatMessage({ id: 'pages.role.save_success', defaultMessage: 'Role saved successfully' }));
        return true;
      }
      message.error(response?.message || intl.formatMessage({ id: 'pages.role.save_fail', defaultMessage: 'Failed to save role' }));
      return false;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({ id: 'pages.role.save_fail', defaultMessage: 'Failed to save role' }));
      return false;
    }
  };

  const handleUpdate = async (values: RoleFormValues) => {
    const hide = message.loading(intl.formatMessage({ id: 'saving', defaultMessage: 'Saving...' }));
    try {
      const response = await updateRole(values);
      hide();
      if (response?.success) {
        message.success(intl.formatMessage({ id: 'pages.role.save_success', defaultMessage: 'Role saved successfully' }));
        return true;
      }
      message.error(response?.message || intl.formatMessage({ id: 'pages.role.save_fail', defaultMessage: 'Failed to save role' }));
      return false;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({ id: 'pages.role.save_fail', defaultMessage: 'Failed to save role' }));
      return false;
    }
  };

  const handleDelete = (role: RoleItem) => {
    confirm({
      icon: <ExclamationCircleOutlined />,
      title: intl.formatMessage({ id: 'pages.role.confirm_delete', defaultMessage: 'Are you sure to delete this role?' }),
      content: `${role.name}`,
      onOk: async () => {
        const hide = message.loading(intl.formatMessage({ id: 'deleting', defaultMessage: 'Deleting...' }));
        try {
          const response = await deleteRole(role.id);
          hide();
          if (response?.success) {
            message.success(intl.formatMessage({ id: 'pages.role.delete_success', defaultMessage: 'Role deleted successfully' }));
            actionRef.current?.reload();
          } else {
            message.error(response?.message || intl.formatMessage({ id: 'pages.role.delete_fail', defaultMessage: 'Failed to delete role' }));
          }
        } catch (error) {
          hide();
          message.error(intl.formatMessage({ id: 'pages.role.delete_fail', defaultMessage: 'Failed to delete role' }));
        }
      },
    });
  };

  const columns: ProColumns<RoleItem>[] = [
    {
      title: intl.formatMessage({ id: 'pages.role.table.cols.name', defaultMessage: 'Name' }),
      dataIndex: 'name',
    },
    {
      title: intl.formatMessage({ id: 'pages.role.table.cols.code', defaultMessage: 'Code' }),
      dataIndex: 'code',
    },
    {
      title: intl.formatMessage({ id: 'pages.role.table.cols.description', defaultMessage: 'Description' }),
      dataIndex: 'description',
      search: false,
    },
    {
      title: intl.formatMessage({ id: 'pages.role.table.cols.system', defaultMessage: 'System Role' }),
      dataIndex: 'isSystem',
      search: false,
      render: (_, record) => (
        <Tag color={record.isSystem ? 'gold' : 'blue'}>
          {record.isSystem
            ? intl.formatMessage({ id: 'pages.role.system.yes', defaultMessage: 'Yes' })
            : intl.formatMessage({ id: 'pages.role.system.no', defaultMessage: 'No' })}
        </Tag>
      ),
    },
    {
      title: intl.formatMessage({ id: 'pages.role.table.cols.functions', defaultMessage: 'Permissions' }),
      dataIndex: 'functions',
      search: false,
      render: (_, record) => (
        <Space size={[0, 4]} wrap>
          {record.functions?.map((fn) => (
            <Tag key={`${record.id}-${fn}`}>{fn}</Tag>
          ))}
        </Space>
      ),
    },
    {
      title: intl.formatMessage({ id: 'pages.role.table.cols.action', defaultMessage: 'Action' }),
      valueType: 'option',
      render: (_, record) => [
        <a
          key="edit"
          onClick={() => {
            setCurrentRole(record);
            setUpdateModalVisible(true);
          }}
        >
          {intl.formatMessage({ id: 'pages.role.table.cols.action.edit', defaultMessage: 'Edit' })}
        </a>,
        !record.isSystem ? (
          <Button key="delete" type="link" danger onClick={() => handleDelete(record)}>
            {intl.formatMessage({ id: 'pages.role.table.cols.action.delete', defaultMessage: 'Delete' })}
          </Button>
        ) : null,
      ],
    },
  ];

  return (
    <PageContainer>
      <ProTable<RoleItem>
        actionRef={actionRef}
        rowKey="id"
        search={false}
        columns={columns}
        request={async () => {
          const response = await queryRoles();
          return {
            data: response?.data || [],
            success: response?.success ?? false,
          };
        }}
        toolBarRender={() => [
          <Button
            key="add"
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setCurrentRole(undefined);
              setCreateModalVisible(true);
            }}
          >
            {intl.formatMessage({ id: 'pages.role.table.cols.action.add', defaultMessage: 'Add Role' })}
          </Button>,
        ]}
      />

      <ModalForm<RoleFormValues>
        title={intl.formatMessage({ id: 'pages.role.form.title.add', defaultMessage: 'Add Role' })}
        visible={createModalVisible}
        onVisibleChange={setCreateModalVisible}
        initialValues={{ functions: [] }}
        onFinish={async (values) => {
          const success = await handleCreate(values);
          if (success) {
            setCreateModalVisible(false);
            actionRef.current?.reload();
          }
          return success;
        }}
      >
        <ProFormText
          name="code"
          label={intl.formatMessage({ id: 'pages.role.form.code', defaultMessage: 'Code' })}
          rules={[{ required: true }]}
        />
        <ProFormText
          name="name"
          label={intl.formatMessage({ id: 'pages.role.form.name', defaultMessage: 'Name' })}
          rules={[{ required: true }]}
        />
        <ProFormText
          name="description"
          label={intl.formatMessage({ id: 'pages.role.form.description', defaultMessage: 'Description' })}
        />
        <ProFormSelect
          name="functions"
          label={intl.formatMessage({ id: 'pages.role.form.functions', defaultMessage: 'Permissions' })}
          mode="multiple"
          options={permissionOptions}
        />
      </ModalForm>

      {updateModalVisible && (
        <ModalForm<RoleFormValues>
          title={intl.formatMessage({ id: 'pages.role.form.title.edit', defaultMessage: 'Edit Role' })}
          visible={updateModalVisible}
          onVisibleChange={(visible) => {
            setUpdateModalVisible(visible);
            if (!visible) {
              setCurrentRole(undefined);
            }
          }}
          initialValues={{
            ...currentRole,
            functions: currentRole?.functions || [],
          }}
          onFinish={async (values) => {
            const success = await handleUpdate({ ...values, id: currentRole?.id, isSystem: currentRole?.isSystem });
            if (success) {
              setUpdateModalVisible(false);
              setCurrentRole(undefined);
              actionRef.current?.reload();
            }
            return success;
          }}
        >
          <ProFormText
            name="code"
            label={intl.formatMessage({ id: 'pages.role.form.code', defaultMessage: 'Code' })}
            disabled={currentRole?.isSystem}
            rules={[{ required: true }]}
          />
          <ProFormText
            name="name"
            label={intl.formatMessage({ id: 'pages.role.form.name', defaultMessage: 'Name' })}
            rules={[{ required: true }]}
          />
          <ProFormText
            name="description"
            label={intl.formatMessage({ id: 'pages.role.form.description', defaultMessage: 'Description' })}
          />
          <ProFormSelect
            name="functions"
            label={intl.formatMessage({ id: 'pages.role.form.functions', defaultMessage: 'Permissions' })}
            mode="multiple"
            options={permissionOptions}
          />
        </ModalForm>
      )}
    </PageContainer>
  );
};

export default RolePage;
