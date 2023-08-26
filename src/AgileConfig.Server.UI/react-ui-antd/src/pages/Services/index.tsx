import { PlusOutlined } from '@ant-design/icons';
import { FormInstance, ModalForm, ProFormDependency, ProFormSelect, ProFormText } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, message, Modal } from 'antd';
import React, {  useRef, useState } from 'react';
import { getIntl, getLocale } from 'umi';
import { ServiceItem } from './data';
import { addService, queryService, removeService } from './service';
import styles from './index.less';
const { confirm } = Modal;

const handleAdd = async (fields: ServiceItem) => {
  const intl = getIntl(getLocale());
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
  const intl = getIntl(getLocale());
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

const services: React.FC = () => {
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  const [createModalVisible, setCreateModalVisible] = useState<boolean>(false);

  const columns: ProColumns[] = [
    {
      title: '服务ID',
      dataIndex: 'serviceId',
    },
    {
      title: '服务名',
      dataIndex: 'serviceName',
      sorter: true,
    },
    {
      title: 'IP',
      dataIndex: 'ip',
      hideInSearch: true,
    },
    {
      title: '端口',
      dataIndex: 'port',
      hideInSearch: true,
    },
    {
      title: '元数据',
      dataIndex: 'metaData',
      hideInSearch: true,
    },
    {
      title: '健康检测模式',
      dataIndex: 'heartBeatMode',
      hideInSearch: true,
    },
    {
      title: '检测 URL',
      dataIndex: 'checkUrl',
      hideInSearch: true,
      ellipsis: true,
    },
    {
      title: '告警 URL',
      dataIndex: 'alarmUrl',
      hideInSearch: true,
      ellipsis: true,
    },
    {
      title: '注册时间',
      dataIndex: 'registerTime',
      hideInSearch: true,
      valueType: 'dateTime',
      sorter: true,
    },
    {
      title: '最后响应时间',
      dataIndex: 'lastHeartBeat',
      hideInSearch: true,
      valueType: 'dateTime',
    },
    {
      title: '状态',
      dataIndex: 'status',
      valueEnum: {
        0: {
          text: '异常',
          status: 'Default'
        },
        1: {
          text: '健康',
          status: 'Success'
        }
      },
      width: 120
    },
    {
      title: '操作',
      valueType: 'option',
      render: (text, record, _, action) => [
        <a className={styles.linkDanger}
          onClick={
            ()=>{
              confirm({
                content:`确定删除选中的服务吗？`,
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
          删除
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
              注册
            </Button>
          ]
        }
      />
      <ModalForm
        formRef={addFormRef}
        title='注册服务'
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
            },
          ]}
          label='服务ID'
          name="serviceId"
        />
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label='服务名'
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
                  label="健康检测模式"
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
            },
          ]}
          label='IP'
          name="ip"
        />
        <ProFormText
          rules={[
            {
            },
          ]}
          label='端口'
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
                  label='检测 URL'
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
                  label='告警 URL'
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
