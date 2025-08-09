import { PlusOutlined } from '@ant-design/icons';
import { FormInstance, ModalForm, ProFormDependency, ProFormSelect, ProFormText } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, message, Modal } from 'antd';
import React, {  useRef, useState } from 'react';
import { useIntl } from 'umi';
import { ServiceItem } from './data';
import { addService, queryService, removeService } from './service';
import styles from './index.less';
const { confirm } = Modal;

const services: React.FC = () => {
  const intl = useIntl();
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  const [createModalVisible, setCreateModalVisible] = useState<boolean>(false);

  const handleAdd = async (fields: ServiceItem) => {
    const hide = message.loading(intl.formatMessage({
      id:'saving'
    }));
    try {
      const result = await addService({ ...fields });
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

  const handleDelSome = async (service: ServiceItem):Promise<boolean> => {
    const hide = message.loading(intl.formatMessage({id:'deleting'}));
    try {
      const result = await removeService(service);
      hide();
      const success = result.success;
      if (success) {
        message.success(intl.formatMessage({id:'delete_success'}));
      } else {
        message.error(intl.formatMessage({id:'delete_fail'}));
      }
      return success;
    } catch (error) {
      hide();
      message.error(intl.formatMessage({id:'delete_fail'}));
      return false;
    }
  };

  const columns: ProColumns[] = [
    {
      title: intl.formatMessage({
        id: 'pages.services.serviceId',
      }),
      dataIndex: 'serviceId',
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.serviceName',
      }),
      dataIndex: 'serviceName',
      sorter: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.ip',
      }),
      dataIndex: 'ip',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.port',
      }),
      dataIndex: 'port',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.metaData',
      }),
      dataIndex: 'metaData',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.heartBeatMode',
      }),
      dataIndex: 'heartBeatMode',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.checkUrl',
      }),
      dataIndex: 'checkUrl',
      hideInSearch: true,
      ellipsis: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.alarmUrl',
      }),
      dataIndex: 'alarmUrl',
      hideInSearch: true,
      ellipsis: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.registerTime',
      }),
      dataIndex: 'registerTime',
      hideInSearch: true,
      valueType: 'dateTime',
      sorter: true,
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.lastHeartBeat',
      }),
      dataIndex: 'lastHeartBeat',
      hideInSearch: true,
      valueType: 'dateTime',
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.status',
      }),
      dataIndex: 'status',
      valueEnum: {
        0: {
          text: intl.formatMessage({
            id: 'pages.services.statusAbnormal',
          }),
          status: 'Default'
        },
        1: {
          text: intl.formatMessage({
            id: 'pages.services.statusHealthy',
          }),
          status: 'Success'
        }
      },
      width: 120
    },
    {
      title: intl.formatMessage({
        id: 'pages.services.operation',
      }),
      valueType: 'option',
      render: (text, record, _, action) => [
        <a className={styles.linkDanger}
          onClick={
            ()=>{
              confirm({
                content: intl.formatMessage({
                  id: 'pages.services.confirmDelete',
                }),
                onOk: async ()=>{
                  const result = await handleDelSome(record)
                  if (result) {
                    actionRef.current?.reload();
                  }
                }
              })
            }
          }
        >
          {intl.formatMessage({
            id: 'pages.services.delete',
          })}
        </a>
      ]
    }
  ];
  return (
    <PageContainer>
      <ProTable
        search={{
          labelWidth: 'auto',
        }}
        actionRef={actionRef}
        options={
          false
        }
        rowKey="id"
        columns={columns}
        request={(params, sorter, filter) => {
          let sortField = 'registerTime';
          let ascOrDesc = 'descend';
          for (const key in sorter) {
            sortField = key;
            const val = sorter[key];
            if (val) {
              ascOrDesc = val;
            }
          }
          console.log(sortField, ascOrDesc);
          return queryService({ sortField, ascOrDesc, ...params })
        }}
        toolBarRender={()=>
          [
            <Button key="button" icon={<PlusOutlined />} type="primary" onClick={() => { setCreateModalVisible(true) }}>
              {intl.formatMessage({
                id: 'pages.services.newService',
              })}
            </Button>
          ]
        }
      />
      <ModalForm
        formRef={addFormRef}
        title={intl.formatMessage({
          id: 'pages.services.newService',
        })}
        visible={createModalVisible}
        onVisibleChange={setCreateModalVisible}
        onFinish={
          async (value) => {
            const success = await handleAdd(value as ServiceItem);
            if (success) {
              setCreateModalVisible(false);
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
              message: intl.formatMessage({
                id: 'pages.services.serviceNameRequired',
              }),
            },
          ]}
          label={intl.formatMessage({
            id: 'pages.services.serviceId',
          })}
          placeholder={intl.formatMessage({
            id: 'pages.services.serviceName.placeholder',
          })}
          name="serviceId"
        />
        <ProFormText
          rules={[
            {
              required: true,
              message: intl.formatMessage({
                id: 'pages.services.serviceNameRequired',
              }),
            },
          ]}
          label={intl.formatMessage({
            id: 'pages.services.serviceName',
          })}
          placeholder={intl.formatMessage({
            id: 'pages.services.serviceName.placeholder',
          })}
          name="serviceName"
        />
        <ProFormSelect
                rules={[
                  {
                    required: true,
                  },
                ]}
                 tooltip={
                   ()=>{
                     return <div>
                       none: 该模式不会进行任何健康检测服务会永远在线<br/>
                       client: 客户端主动上报<br/>
                       server: 服务端主动检测
                     </div>
                   }
                 }
                  label={intl.formatMessage({
                    id: 'pages.services.heartBeatMode',
                  })}
                  name="heartBeatMode"
                  request={ async () => {
                    return [
                      {
                        label:'none',
                        value: 'none',
                      },
                      {
                        label:'client',
                        value: 'client'
                      },
                      {
                        label:'server',
                        value: 'server'
                      }
                    ];
                  }}
        >
        </ProFormSelect>
        <ProFormText
          rules={[
            {
              required: true,
              message: intl.formatMessage({
                id: 'pages.services.ipRequired',
              }),
            },
          ]}
          label={intl.formatMessage({
            id: 'pages.services.ip',
          })}
          placeholder={intl.formatMessage({
            id: 'pages.services.ip.placeholder',
          })}
          name="ip"
        />
        <ProFormText
          rules={[
            {
              required: true,
              message: intl.formatMessage({
                id: 'pages.services.portRequired',
              }),
            },
          ]}
          label={intl.formatMessage({
            id: 'pages.services.port',
          })}
          placeholder={intl.formatMessage({
            id: 'pages.services.port.placeholder',
          })}
          name="port"
        />
        <ProFormDependency
          name={
            ["heartBeatMode"]
          }
        >
          {
            (e)=>{
              return e.heartBeatMode == 'server'? <ProFormText
                  rules={[
                    {
                      required: true,
                    },
                  ]}
                  label={intl.formatMessage({
                    id: 'pages.services.checkUrl',
                  })}
                  placeholder={intl.formatMessage({
                    id: 'pages.services.checkUrl.placeholder',
                  })}
                  name="checkUrl"
                />: null
            }
          }
        </ProFormDependency>
        <ProFormDependency
          name={
            ["heartBeatMode"]
          }
        >
          {
            (e)=>{
              return (e.heartBeatMode == 'none' || e.heartBeatMode == null)? null:<ProFormText
                  rules={[
                    {
                    },
                  ]}
                  label={intl.formatMessage({
                    id: 'pages.services.alarmUrl',
                  })}
                  placeholder={intl.formatMessage({
                    id: 'pages.services.alarmUrl.placeholder',
                  })}
                  name="alarmUrl"
                />
            }
          }
        </ProFormDependency>
      </ModalForm>
    </PageContainer>
  );
}
export default services;
