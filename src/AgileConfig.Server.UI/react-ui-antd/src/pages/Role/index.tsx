import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, message, Modal, Space, Tag } from 'antd';
import { ModalForm, ProFormSelect, ProFormText } from '@ant-design/pro-form';
import type { CustomTagProps } from 'rc-select/lib/BaseSelect';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import { useIntl } from 'umi';
import type { RoleFormValues, RoleItem } from './data';
import { createRole, deleteRole, fetchSupportedRolePermissions, queryRoles, updateRole } from '@/services/role';
import { RequireFunction } from '@/utils/permission';

const { confirm } = Modal;

const PERMISSION_GROUP_COLORS: Record<string, string> = {
  APP: 'blue',
  CONFIG: 'geekblue',
  NODE: 'purple',
  CLIENT: 'green',
  USER: 'gold',
  ROLE: 'volcano',
  SERVICE: 'cyan',
  LOG: 'magenta',
  OTHER: 'default',
};

const getPermissionTagColor = (permission: string) => {
  const prefix = permission.split('_')[0];
  return PERMISSION_GROUP_COLORS[prefix] || PERMISSION_GROUP_COLORS.OTHER;
};

const renderPermissionTag = (props: CustomTagProps) => {
  const { label, value, closable, onClose } = props;
  const onPreventMouseDown = (event: React.MouseEvent<HTMLSpanElement>) => {
    event.preventDefault();
    event.stopPropagation();
  };
  return (
    <Tag
      key={String(value)} {...props}
      color={getPermissionTagColor(String(value))}
      onMouseDown={onPreventMouseDown}
      closable={closable}
      onClose={onClose}
    >
      {label}
    </Tag>
  );
};

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

  const permissionGroupLabel = (prefix: string) => {
    return intl.formatMessage({
      id: `pages.role.permissionGroup.${prefix}`,
      defaultMessage: prefix,
    });
  };

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

  const groupedPermissionOptions = useMemo(() => {
    const groupMap: Record<
      string,
      {
        label: string;
        options: { value: string; label: string }[];
      }
    > = {};

    supportedPermissions.forEach((item) => {
      const prefix = item.split('_')[0] || 'OTHER';
      if (!groupMap[prefix]) {
        groupMap[prefix] = {
          label: permissionGroupLabel(prefix),
          options: [],
        };
      }
      groupMap[prefix].options.push({
        value: item,
        label: intl.formatMessage({ id: `pages.role.permissions.${item}`, defaultMessage: item }),
      });
    });

    return Object.values(groupMap);
  }, [intl, supportedPermissions]);

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
      render: (_, record) => {
        const fns = record.functions || [];
        const hasAll = supportedPermissions.length > 0 && supportedPermissions.every(p => fns.includes(p));
        if (hasAll) {
          return <Tag>{intl.formatMessage({ id: 'pages.role.permissions.all', defaultMessage: '所有权限' })}</Tag>;
        }
        return (
          <Space size={[0, 4]} wrap>
            {fns.map((fn) => {
              return (
                <Tag color={getPermissionTagColor(fn)} key={`${record.id}-${fn}`}>
                  {intl.formatMessage({ id: `pages.role.permissions.${fn}`, defaultMessage: fn })}
                </Tag>
              );
            })}
          </Space>
        );
      },
    },
    {
      title: intl.formatMessage({ id: 'pages.role.table.cols.action', defaultMessage: 'Action' }),
      valueType: 'option',
      width: 160,
      render: (_, record) => [
        <RequireFunction fn="ROLE_EDIT" key="edit" fallback={null}>
          <a
            onClick={() => {
              setCurrentRole(record);
              setUpdateModalVisible(true);
            }}
          >
            {intl.formatMessage({ id: 'pages.role.table.cols.action.edit', defaultMessage: 'Edit' })}
          </a>
        </RequireFunction>,
        !record.isSystem ? (
          <RequireFunction fn="ROLE_DELETE" key="delete" fallback={null}>
            <Button type="link" danger onClick={() => handleDelete(record)}>
              {intl.formatMessage({ id: 'pages.role.table.cols.action.delete', defaultMessage: 'Delete' })}
            </Button>
          </RequireFunction>
        ) : null,
      ].filter(Boolean),
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
          const data = response?.data || [];
          // filter super admin role
          const filteredData = data.filter((role: RoleItem) => role.name !== 'Super Administrator');
          return {
            data: filteredData,
            success: response?.success ?? false,
          };
        }}
        toolBarRender={() => [
          <RequireFunction fn="ROLE_ADD" key="add" fallback={null}>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => {
                setCurrentRole(undefined);
                setCreateModalVisible(true);
              }}
            >
              {intl.formatMessage({ id: 'pages.role.table.cols.action.add', defaultMessage: 'Add Role' })}
            </Button>
          </RequireFunction>,
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
          options={groupedPermissionOptions}
          fieldProps={{
            tagRender: renderPermissionTag,
          }}
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
            options={groupedPermissionOptions}
            fieldProps={{
              tagRender: renderPermissionTag,
            }}          />
        </ModalForm>
      )}
    </PageContainer>
  );
};

export default RolePage;
