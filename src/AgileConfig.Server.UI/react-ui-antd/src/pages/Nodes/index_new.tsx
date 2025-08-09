import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { ModalForm, ProFormText, ProFormTextArea } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, FormInstance, message,Modal } from 'antd';
import React, { useState, useRef } from 'react';
import { NodeItem } from './data';
import { queryNodes, addNode, delNode,allClientReload } from './service';
import { useIntl } from 'umi';
import AuthorizedEle from '@/components/Authorized/AuthorizedElement';
import functionKeys from '@/models/functionKeys';

const { confirm } = Modal;

const nodeList:React.FC = () => {
  const intl = useIntl();
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  const [createModalVisible, handleModalVisible] = useState<boolean>(false);

  const handleAdd = async (fields: NodeItem) => {
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
        message.error(result.message);
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

  const columns: ProColumns<NodeItem>[] = [
    {
      title: intl.formatMessage({
        id: 'pages.nodes.address',
      }),
      dataIndex: 'address',
    },
    {
      title: intl.formatMessage({
        id: 'pages.nodes.remark',
      }),
      dataIndex: 'remark',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.nodes.status',
      }),
      dataIndex: 'status',
      hideInForm: true,
      valueEnum: {
        '0': {
          text: intl.formatMessage({
            id: 'pages.nodes.statusOffline',
          }),
          status: 'Default',
        },
        '1': {
          text: intl.formatMessage({
            id: 'pages.nodes.statusOnline',
          }),
          status: 'Processing',
        },
      },
    },
    {
      title: intl.formatMessage({
        id: 'pages.nodes.lastEchoTime',
      }),
      dataIndex: 'lastEchoTime',
      hideInSearch: true,
      valueType: 'dateTime',
    },
    {
      title: intl.formatMessage({
        id: 'pages.nodes.createTime',
      }),
      dataIndex: 'createTime',
      hideInSearch: true,
      valueType: 'dateTime',
    },
    {
      title: intl.formatMessage({
        id: 'pages.nodes.operation',
      }),
      dataIndex: 'option',
      valueType: 'option',
      render: (_, record) => [
        <AuthorizedEle key="del" judgeKey={functionKeys.Node_Delete}>
          <a
            onClick={() => {
              confirm({
                title: intl.formatMessage({
                  id: 'pages.nodes.confirmDelete',
                }),
                icon: <ExclamationCircleOutlined />,
                content: intl.formatMessage({
                  id: 'pages.nodes.confirmDeleteContent',
                }),
                onOk() {
                  handleDel(record).then((success) => {
                    if (success) {
                      if (actionRef.current) {
                        actionRef.current.reload();
                      }
                    }
                  });
                },
                onCancel() {},
              });
            }}
          >
            {intl.formatMessage({
              id: 'pages.nodes.delete',
            })}
          </a>
        </AuthorizedEle>,
      ],
    },
  ];

  return (
    <PageContainer>
      <ProTable<NodeItem>
        headerTitle={intl.formatMessage({
          id: 'pages.nodes.title',
        })}
        actionRef={actionRef}
        rowKey="id"
        search={{
          labelWidth: 120,
        }}
        toolBarRender={() => [
          <AuthorizedEle key="reload" judgeKey={functionKeys.Node_Add}>
            <Button type="primary" onClick={() => {
              confirm({
                title: intl.formatMessage({
                  id: 'pages.nodes.reloadAll',
                }),
                icon: <ExclamationCircleOutlined />,
                content: intl.formatMessage({
                  id: 'pages.nodes.reloadAllContent',
                }),
                onOk() {
                  handleAllReload({} as NodeItem).then((success) => {
                    
                  });
                },
                onCancel() {},
              });
            }}>
              {intl.formatMessage({
                id: 'pages.nodes.reloadAll',
              })}
            </Button>
          </AuthorizedEle>,
          <AuthorizedEle key="new" judgeKey={functionKeys.Node_Add}>
            <Button
              type="primary"
              key="primary"
              onClick={() => {
                handleModalVisible(true);
              }}
            >
              <PlusOutlined /> {intl.formatMessage({
                id: 'pages.nodes.new',
              })}
            </Button>
          </AuthorizedEle>,
        ]}
        request={queryNodes}
        columns={columns}
      />
      <ModalForm
        formRef={addFormRef}
        title={intl.formatMessage({
          id: 'pages.nodes.new',
        })}
        visible={createModalVisible}
        onVisibleChange={handleModalVisible}
        onFinish={async (value) => {
          const success = await handleAdd(value as NodeItem);
          if (success) {
            handleModalVisible(false);
            if (actionRef.current) {
              actionRef.current.reload();
            }
          }
        }}
      >
        <ProFormText
          rules={[
            {
              required: true,
              message: intl.formatMessage({
                id: 'pages.nodes.addressRequired',
              }),
            },
          ]}
          label={intl.formatMessage({
            id: 'pages.nodes.address',
          })}
          placeholder={intl.formatMessage({
            id: 'pages.nodes.addressPlaceholder',
          })}
          name="address"
        />
        <ProFormTextArea 
          label={intl.formatMessage({
            id: 'pages.nodes.remark',
          })} 
          placeholder={intl.formatMessage({
            id: 'pages.nodes.remarkPlaceholder',
          })}
          name="remark" 
        />
      </ModalForm>
    </PageContainer>
  );
};

export default nodeList;
