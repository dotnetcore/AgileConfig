import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, FormInstance, message,Modal } from 'antd';
import React, { useState, useRef } from 'react';
import { UserItem } from './data';
import { queryUsers, addUser, delUser, editUser } from './service';
import { useIntl, getIntl, getLocale } from 'umi';
import { ModalForm, ProFormText, ProFormTextArea } from '@ant-design/pro-form';

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
      message.error(intl.formatMessage({
        id: 'save_fail'
      }));
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

const nodeList:React.FC = () => {
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  const intl = useIntl();

  const [createModalVisible, handleModalVisible] = useState<boolean>(false);
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
      title: intl.formatMessage({
        id: 'pages.node.table.cols.action'
      }),
      valueType: 'option',
      render: (text, record, _, action) => [
        <Button  type="link" danger
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
        actionRef={actionRef}
        columns = {columns}
        request = { (params, sorter, filter) => queryUsers(params) }
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary"
          onClick={ ()=>{ handleModalVisible(true) } }
          >
            {
              intl.formatMessage({
                id: 'pages.node.action.add'
              })
            }
          </Button>
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
          width="md"
          name="userName" 
        />
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label= "密码"
          width="md"
          name="password" 
        />
       <ProFormText
          label= "团队"
          width="md"
          name="team" 
        />
      </ModalForm>

    </PageContainer>
  );
}
export default nodeList;
