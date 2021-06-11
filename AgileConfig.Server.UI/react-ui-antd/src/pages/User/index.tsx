import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, FormInstance, message,Modal, Space, Tag } from 'antd';
import React, { useState, useRef } from 'react';
import { UserItem } from './data';
import { queryUsers, addUser, delUser, editUser, resetPassword } from './service';
import { useIntl, getIntl, getLocale } from 'umi';
import { ModalForm, ProFormSelect, ProFormText } from '@ant-design/pro-form';
import UpdateUser from './comps/updateUser';
import { getAuthority } from '@/utils/authority';

const { confirm } = Modal;
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
  const authMap = { 'SuperAdmin': 0,'Admin':1,'NormalUser':2};
  let currentAuthNum = 2;
  const roles = getAuthority();
  if (Array.isArray(roles)) {
    let max = roles.map(x=> authMap[x]).sort((a, b) => a - b)[0];
    currentAuthNum = max;
  }
  let userAuthNum = user.userRoles.sort((a, b) => a - b)[0];

  return currentAuthNum < userAuthNum;
}

const userList:React.FC = () => {
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  const intl = useIntl();

  const [createModalVisible, handleModalVisible] = useState<boolean>(false);
  const [updateModalVisible, setUpdateModalVisible] = useState<boolean>(false);
  const [currentRow, setCurrentRow] = useState<UserItem>();
  const columns: ProColumns<UserItem>[] = [
    {
      title: '用户名',
      dataIndex: 'userName',
    },
    {
      title: '团队',
      dataIndex: 'team',
    },
    {
      title: '角色',
      dataIndex: 'userRoles',
      search: false,
      renderFormItem: (_, { defaultRender }) => {
        return defaultRender(_);
      },
      render: (_, record) => (
        <Space>
          {record.userRoleNames?.map((name:string) => (
            <Tag color={
              name === '管理员'? 'gold':'blue'
            } key={name}>
              {name}
            </Tag>
          ))}
        </Space>
      ),
    },
    {
      title: intl.formatMessage({
        id: 'pages.node.table.cols.action'
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
          修改
        </a>,
         <a key="1"
         onClick={ ()=> {
          const msg = '确定重置用户' + `【${record.userName}】的密码为默认密码【123456】?`;
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
         重置密码
         </a>,
        <Button key="2" type="link" danger
          onClick={ ()=> {
            const msg = '确定删除用户' + `【${record.userName}】?`;
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
          删除
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
            {
              intl.formatMessage({
                id: 'pages.node.action.add'
              })
            }
          </Button>
          :
          <span key="1"></span>
        ]}
      />

    <ModalForm 
        formRef={addFormRef}
        title={
          intl.formatMessage({
            id: 'pages.node.action.add'
          })
        } 
        width="400px"
        visible={createModalVisible}
        onVisibleChange={handleModalVisible}
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
          label= "用户名"
          name="userName" 
        />
        <ProFormText.Password
          rules={[
            {
              required: true,
            },
          ]}
          label= "密码"
          name="password" 
        />
       <ProFormText
          label= "团队"
          name="team" 
        />
        <ProFormSelect
                rules={[
                  {
                    required: true,
                  },
                ]}
                  label="角色"
                  name="userRoles"
                  mode="multiple" 
                  options = {hasUserRole('SuperAdmin')?[
                    {
                      value: 1,
                      label: '管理员',
                    },
                    {
                      value: 2,
                      label: '操作员',
                    }
                  ]:[
                  {
                    value: 2,
                    label: '操作员',
                  }]}
                >
        </ProFormSelect> 
      </ModalForm>

      {
        updateModalVisible &&
        <UpdateUser
          value={currentRow}
          setValue={setCurrentRow}
          updateModalVisible={updateModalVisible}
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
              addFormRef.current?.resetFields();
            }
          }/>
      }

    </PageContainer>
  );
}
export default userList;
