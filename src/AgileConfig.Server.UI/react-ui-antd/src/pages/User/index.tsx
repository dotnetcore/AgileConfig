import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, FormInstance, message,Modal, Space, Tag } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { UserItem } from './data';
import { queryUsers, addUser, delUser, editUser, resetPassword } from './service';
import { queryRoles } from '@/services/role';
import { useIntl, getIntl, getLocale } from 'umi';
import { ModalForm, ProFormSelect, ProFormText } from '@ant-design/pro-form';
import UpdateUser from './comps/updateUser';
import { getAuthority } from '@/utils/authority';

const { confirm } = Modal;

type RoleOption = {
  value: string;
  label: string;
  code: string;
  isSystem: boolean;
};
const handleAdd = async (fields: UserItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'saving'
  }));
  try {
    const result = await addUser({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id: 'save_success'
      }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id: 'save_fail'
    }));
    return false;
  }
};
const handleEdit = async (user: UserItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id:'saving'
  }));
  try {
    const result = await editUser({ ...user });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id:'save_success'
      }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id:'save_fail'
    }));
    return false;
  }
};
const handleDel = async (fields: UserItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'deleting'
  }));
  try {
    const result = await delUser(fields.id);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id: 'delete_success'
      }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id: 'delete_fail'
    }));
    return false;
  }
};

const handleResetPassword = async (fields: UserItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'deleting'
  }));
  try {
    const result = await resetPassword(fields.id);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id: 'delete_success'
      }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id: 'delete_fail'
    }));
    return false;
  }
};

const hasUserRole = (role:string) => {
  const authority = getAuthority();
  if (Array.isArray(authority)) {
    if (authority.find(x=> x === role)) {
      return true;
    }
  }

  return false;
}

const checkUserListModifyPermission = (user:UserItem) => {
  // Lower number means higher privilege
  const authMap:Record<string, number> = { SuperAdmin: 0, Admin: 1, NormalUser: 2 };
  const myRoles = getAuthority();
  if (!Array.isArray(myRoles) || myRoles.length === 0) return false;

  // If current user is SuperAdmin -> can edit anyone except themselves (optional)
  if (myRoles.includes('SuperAdmin')) {
    // Prevent editing own account if desired
    if (user.userName && user.userName === (typeof localStorage !== 'undefined' ? localStorage.getItem('currentUserName') : undefined)) {
      return false; // disallow self-edit via list (modal still available maybe)
    }
    return true;
  }

  // Determine current user's minimal privilege level
  const currentAuthNum = myRoles
    .map(r => authMap[r] ?? 999)
    .reduce((min, v) => v < min ? v : min, 999);

  // Determine target user's minimal privilege level
  const targetCodes = user.userRoleCodes || [];
  const userAuthNum = targetCodes.length > 0
    ? targetCodes.map(c => authMap[c] ?? 999).reduce((min, v) => v < min ? v : min, 999)
    : 999;

  // Allow edit only if current privilege strictly higher than target (numerically lower)
  return currentAuthNum < userAuthNum;
}

const userList:React.FC = () => {
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  const intl = useIntl();

  const [createModalVisible, handleModalVisible] = useState<boolean>(false);
  const [updateModalVisible, setUpdateModalVisible] = useState<boolean>(false);
  const [currentRow, setCurrentRow] = useState<UserItem>();
  const [roleOptions, setRoleOptions] = useState<RoleOption[]>([]);

  useEffect(() => {
    loadRoles();
  }, []);

  const loadRoles = async () => {
    try {
      const response = await queryRoles();
      if (response?.success && Array.isArray(response.data)) {
        const options = response.data.map((role: any) => ({
          value: role.id,
          label: role.name,
          code: role.code,
          isSystem: role.isSystem,
        }));
        setRoleOptions(options);
      } else {
        message.error(intl.formatMessage({ id: 'pages.role.load_failed', defaultMessage: 'Failed to load roles' }));
      }
    } catch (error) {
      message.error(intl.formatMessage({ id: 'pages.role.load_failed', defaultMessage: 'Failed to load roles' }));
    }
  };

  const getDefaultRoleIds = () => {
    const normalRole = roleOptions.find(option => option.code === 'NormalUser');
    return normalRole ? [normalRole.value] : [];
  };

  const availableRoleOptions = hasUserRole('SuperAdmin')
    ? roleOptions
    : roleOptions.filter(option => option.code !== 'SuperAdmin');
  const columns: ProColumns<UserItem>[] = [
    {
      title: intl.formatMessage({
        id: 'pages.user.table.cols.username',
      }),
      dataIndex: 'userName',
    },
    {
      title: intl.formatMessage({
        id: 'pages.user.table.cols.team',
      }),
      dataIndex: 'team',
    },
    {
      title: intl.formatMessage({
        id: 'pages.user.table.cols.userrole',
      }),
      dataIndex: 'userRoleNames',
      search: false,
      renderFormItem: (_, { defaultRender }) => {
        return defaultRender(_);
      },
      render: (_, record) => (
        <Space>
          {record.userRoleNames?.map((name:string, index:number) => {
            const code = record.userRoleCodes?.[index];
            let color = 'blue';
            if (code === 'SuperAdmin') {
              color = 'red';
            } else if (code === 'Admin') {
              color = 'gold';
            }
            return (
              <Tag color={color} key={`${record.id}-${code || name}`}>
                {name}
              </Tag>
            );
          })}
        </Space>
      ),
    },
    {
      title: intl.formatMessage({
        id: 'pages.user.table.cols.action'
      }),
      valueType: 'option',
      render: (text, record, _, action) => checkUserListModifyPermission(record)?[
        <a key="0"
        onClick={() => {
          setUpdateModalVisible(true);
          setCurrentRow(record);
          console.log('select user ', record);
          console.log('current user ', currentRow);
        }}
        >
          {intl.formatMessage({
            id: 'pages.user.table.cols.action.edit',
          })}
        </a>,
         <a key="1"
         onClick={ ()=> {
          const msg = intl.formatMessage({
            id: 'pages.user.confirm_reset',
          }) + `【${record.userName}】` + intl.formatMessage({
            id: 'pages.user.reset_password_default',
          });
          confirm({
            icon: <ExclamationCircleOutlined />,
            content: msg,
            async onOk() {
              console.log('reset password user ' + record.userName);
              const success = await handleResetPassword(record);
              if (success) {
                actionRef.current?.reload();
              }
            },
            onCancel() {
              console.log('Cancel');
            },
          });
        }}
         >
         {intl.formatMessage({
           id: 'pages.user.table.cols.action.reset',
         })}
         </a>,
        <Button key="2" type="link" danger
          onClick={ ()=> {
            const msg = intl.formatMessage({
              id: 'pages.user.confirm_delete',
            }) + `【${record.userName}】?`;
            confirm({
              icon: <ExclamationCircleOutlined />,
              content: msg,
              async onOk() {
                console.log('delete user ' + record.userName);
                const success = await handleDel(record);
                if (success) {
                  actionRef.current?.reload();
                }
              },
              onCancel() {
                console.log('Cancel');
              },
            });
          }}
        >
          {intl.formatMessage({
            id: 'pages.user.table.cols.action.delete',
          })}
        </Button >
      ]:[]
    }
  ];
  return (
    <PageContainer>
      <ProTable     
      options={
        false
      }
        rowKey="id"
        actionRef={actionRef}
        columns = {columns}
        request = { (params, sorter, filter) => queryUsers(params) }
        toolBarRender={() => [
          (hasUserRole('SuperAdmin')||hasUserRole('Admin'))? 
          <Button key="0" icon={<PlusOutlined />} type="primary"
          onClick={ ()=>{ handleModalVisible(true) } }
          >
            {intl.formatMessage({
              id: 'pages.user.table.cols.action.add'
            })}
          </Button>
          :
          <span key="1"></span>
        ]}
      />

    <ModalForm 
        modalProps={
          {
            maskClosable: false
          }
        }
        formRef={addFormRef}
        title={
          intl.formatMessage({
            id: 'pages.user.form.title.add'
          })
        } 
        width="400px"
        visible={createModalVisible}
        initialValues={{
          userRoleIds: getDefaultRoleIds(),
        }}
        onVisibleChange={(visible) => {
          handleModalVisible(visible);
          if (visible) {
            addFormRef.current?.setFieldsValue({ userRoleIds: getDefaultRoleIds() });
          } else {
            addFormRef.current?.resetFields();
          }
        }}
        onFinish={
          async (value) => {
            const success = await handleAdd(value as UserItem);
            if (success) {
              handleModalVisible(false);
              if (actionRef.current) {
                actionRef.current.reload();
              }
            }
            addFormRef.current?.resetFields();
          }
        }
      >
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label={intl.formatMessage({
            id: 'pages.user.form.username'
          })}
          name="userName" 
        />
        <ProFormText.Password
          rules={[
            {
              required: true,
            },
          ]}
          label={intl.formatMessage({
            id: 'pages.user.form.password'
          })}
          name="password" 
        />
       <ProFormText
          label={intl.formatMessage({
            id: 'pages.user.form.team'
          })}
          name="team" 
        />
        <ProFormSelect
                rules={[
                  {
                    required: true,
                  },
                ]}
                  label={intl.formatMessage({
                    id: 'pages.user.form.userrole'
                  })}
                  name="userRoleIds"
                  mode="multiple"
                  options = {availableRoleOptions}
                >
        </ProFormSelect>
      </ModalForm>

      {
        updateModalVisible &&
        <UpdateUser
          value={currentRow}
          setValue={setCurrentRow}
          updateModalVisible={updateModalVisible}
          roleOptions={availableRoleOptions}
          defaultRoleIds={getDefaultRoleIds()}
          onCancel={
            () => {
              setCurrentRow(undefined);
              setUpdateModalVisible(false);
            }
          }
          onSubmit={
            async (value) => {
              setCurrentRow(undefined);
              const success = await handleEdit(value);
              if (success) {
                setUpdateModalVisible(false);
                if (actionRef.current) {
                  actionRef.current.reload();
                }
              }
            }
          }
        />
     }

    </PageContainer>
  );
}
export default userList;
