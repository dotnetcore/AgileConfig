import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { ModalForm, ProFormText, ProFormTextArea } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, FormInstance, message,Modal } from 'antd';
import React, { useState, useRef } from 'react';
import { NodeItem } from './data';
import { queryNodes, addNode, delNode,allClientReload } from './service';
import { useIntl, getIntl, getLocale } from 'umi';

const { confirm } = Modal;
const handleAdd = async (fields: NodeItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'saving'
  }));
  try {
    const result = await addNode({ ...fields });
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
const handleAllReload = async (fields: NodeItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'refreshing'
  }));
  try {
    const result = await allClientReload({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id: 'refresh_success'
      }));
    } else {
      message.error(intl.formatMessage({
        id: 'refresh_fail'
      }));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id: 'refresh_fail'
    }));
    return false;
  }
};
const handleDel = async (fields: NodeItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'deleting'
  }));
  try {
    const result = await delNode({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id: 'delete_success'
      }));
    } else {
      message.error(intl.formatMessage({
        id: 'delete_fail'
      }));
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
  const columns: ProColumns<NodeItem>[] = [
    {
      title: intl.formatMessage({
        id: 'pages.node.table.cols.address'
      }),
      dataIndex: 'address',
    },
    {
      title: intl.formatMessage({
        id: 'pages.node.table.cols.remark'
      }),
      dataIndex: 'remark',
    },
    {
      title: intl.formatMessage({
        id: 'pages.node.table.cols.lastEcho'
      }),
      dataIndex: 'lastEchoTime',
      valueType: 'dateTime'
    },
    {
      title: intl.formatMessage({
        id: 'pages.node.table.cols.status'
      }),
      dataIndex: 'status',
      valueEnum: {
        1:{
          text:  intl.formatMessage({
            id: 'pages.node.table.cols.status.1'
          }),
          status: 'Processing'
        },
        0: {
          text:  intl.formatMessage({
            id: 'pages.node.table.cols.status.0'
          }),
          status: 'Default'
        }
      }
    },
    {
      title: intl.formatMessage({
        id: 'pages.node.table.cols.action'
      }),
      valueType: 'option',
      render: (text, record, _, action) => [
        <a onClick={
          ()=>{
            handleAllReload(record)
          }
        }>
          {
            intl.formatMessage({
              id: 'pages.node.action.refresh'
            })
          }
        </a>,
        <Button  type="link" danger
          onClick={ ()=> {
            const msg = intl.formatMessage({id: 'pages.node.delete_msg'}) + `【${record.address}】?`;
            confirm({
              icon: <ExclamationCircleOutlined />,
              content: <div>
                          <div>{msg}</div>
                          <br></br>
                          <div>
                            {
                              intl.formatMessage({
                                id: 'pages.node.action.delete.tips'
                              })
                            }
                          </div>
                        </div>,
              async onOk() {
                console.log('delete node ' + record.address);
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
          {
              intl.formatMessage({
                id: 'pages.node.action.delete'
              })
          }
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
        rowKey="address"
        actionRef={actionRef}
        columns = {columns}
        search={false}
        request = { (params, sorter, filter) => queryNodes() }
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
            const success = await handleAdd(value as NodeItem);
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
          label={
            intl.formatMessage({
              id: 'pages.node.table.cols.address'
            })
          }
          width="md"
          name="address" 
          tooltip={
            intl.formatMessage({
              id: 'pages.node.from.tips'
            })
          }
        />
        <ProFormTextArea
          label={
            intl.formatMessage({
              id: 'pages.node.table.cols.remark'
            })
          }
          width="md"
          name="remark"
        />
      </ModalForm>
    </PageContainer>
  );
}
export default nodeList;
