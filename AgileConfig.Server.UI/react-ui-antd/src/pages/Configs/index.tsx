import { PlusOutlined } from '@ant-design/icons';
import { ModalForm, ProFormSwitch, ProFormText, ProFormTextArea } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button, Drawer, FormInstance, List, message, Modal, Tag } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { queryApps } from '../Apps/service';
import UpdateForm from './comps/updateForm';
import { ConfigListItem, ConfigModifyLog } from './data';
import { queryConfigs, onlineConfig, offlineConfig, delConfig, addConfig, editConfig, queryModifyLogs,rollback,onlineSomeConfigs,offlineSomeConfigs } from './service';
import Text from 'antd/lib/typography/Text';
import moment from 'moment';
import styles from './index.less';

const { confirm } = Modal;

const handleOnline = async (fields: ConfigListItem) => {
  const hide = message.loading('正在上线');
  try {
    const result = await onlineConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('上线成功');
    } else {
      message.error('上线失败请重试！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('上线失败请重试！');
    return false;
  }
};

const handleOnlineSome = async (configs: ConfigListItem[]) => {
  const hide = message.loading('正在上线');
  try {
    const result = await onlineSomeConfigs(configs);
    hide();
    const success = result.success;
    if (success) {
      message.success('上线成功');
    } else {
      message.error('上线失败请重试！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('上线失败请重试！');
    return false;
  }
};

const handleOfflineSome = async (configs: ConfigListItem[]) => {
  const hide = message.loading('正在下线');
  try {
    const result = await offlineSomeConfigs(configs);
    hide();
    const success = result.success;
    if (success) {
      message.success('下线成功');
    } else {
      message.error('下线失败请重试！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('下线失败请重试！');
    return false;
  }
};

const handleOffline = async (fields: ConfigListItem) => {
  const hide = message.loading('正在下线');
  try {
    const result = await offlineConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('下线成功');
    } else {
      message.error('下线失败请重试！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('下线失败请重试！');
    return false;
  }
};

const handleDel = async (fields: ConfigListItem) => {
  const hide = message.loading('正在删除');
  try {
    const result = await delConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('删除成功');
    } else {
      message.error('删除失败请重试！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('删除失败请重试！');
    return false;
  }
};
const handleAdd = async (fields: ConfigListItem) => {
  const hide = message.loading('正在保存');
  try {
    const result = await addConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success('新建成功');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('新建失败请重试！');
    return false;
  }
};
const handleEdit = async (config: ConfigListItem) => {
  const hide = message.loading('正在保存');
  try {
    const result = await editConfig({ ...config });
    hide();
    const success = result.success;
    if (success) {
      message.success('保存成功');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('保存失败请重试！');
    return false;
  }
};
const handleRollback = async (config: ConfigModifyLog) => {
  const hide = message.loading('正在回滚');
  try {
    const result = await rollback({ ...config });
    hide();
    const success = result.success;
    if (success) {
      message.success('回滚成功');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('回滚失败请重试！');
    return false;
  }
};
const configs: React.FC = (props: any) => {
  const appId = props.match.params.app_id;
  const appName = props.match.params.app_name;
  const [appEnums, setAppEnums] = useState<any>();
  const [createModalVisible, setCreateModalVisible] = useState<boolean>(false);
  const [updateModalVisible, setUpdateModalVisible] = useState<boolean>(false);
  const [modifyLogsModalVisible, setmodifyLogsModalVisible] = useState<boolean>(false);
  const [currentRow, setCurrentRow] = useState<ConfigListItem>();
  const [selectedRowsState, setSelectedRows] = useState<ConfigListItem[]>([]);
  const [modifyLogs, setModifyLogs] = useState<ConfigModifyLog[]>([]);
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  const getAppEnums = async () => {
    const result = await queryApps({})
    const obj = {};
    result.data.forEach((x) => {
      obj[x.id] = {
        text: x.name
      }
    });

    return obj;
  }
  useEffect(() => {
    getAppEnums().then(x => {
      console.log('app enums ', x);
      setAppEnums({ ...x });
    });
  }, []);
  const online = (config: ConfigListItem) => {
    confirm({
      content: `确定上线配置【${config.key}】？`,
      onOk: async () => {
        const result = await handleOnline(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const onlineSome = (configs: ConfigListItem[]) => {
    const waitPublishConfigs = configs.filter(x=>x.onlineStatus === 0);
    if (!waitPublishConfigs.length) {
      message.warning('请至少选中一个待上线配置项');
      return;
    }
    confirm({
      content: `确定上线选中的配置？`,
      onOk: async () => {
        const result = await handleOnlineSome(configs);
        if (result && actionRef.current) {
          if (actionRef.current?.clearSelected){
            actionRef.current?.clearSelected();
          }
          actionRef.current?.reload();
        }
      }
    });
  }
  const offlineSome = (configs: ConfigListItem[]) => {
    const waitPublishConfigs = configs.filter(x=>x.onlineStatus === 1);
    if (!waitPublishConfigs.length) {
      message.warning('请至少选中一个已上线配置项');
      return;
    }
    confirm({
      content: `确定下线选中的配置？`,
      onOk: async () => {
        const result = await handleOfflineSome(configs);
        if (result && actionRef.current) {
          if (actionRef.current?.clearSelected){
            actionRef.current?.clearSelected();
          }
          actionRef.current?.reload();
        }
      }
    });
  }
  const offline = (config: ConfigListItem) => {
    confirm({
      content: `确定下线配置【${config.key}】？`,
      onOk: async () => {
        const result = await handleOffline(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const delConfig = (config: ConfigListItem) => {
    confirm({
      content: `确定删除配置【${config.key}】？`,
      onOk: async () => {
        const result = await handleDel(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const rollback = (config: ConfigModifyLog) => {
    confirm({
      content: `确定回滚至版本【${moment(config.modifyTime).format('YYYY-MM-DD HH:mm:ss')}】？`,
      onOk: async () => {
        const result = await handleRollback(config);
        if (result) {
          setmodifyLogsModalVisible(false);
          actionRef.current?.reload();
        }
      }
    });
  }

  const columns: ProColumns<ConfigListItem>[] = [
    {
      title: '组',
      dataIndex: 'group',
    },
    {
      title: '键',
      dataIndex: 'key',
      copyable: true
    },
    {
      title: '值',
      dataIndex: 'value',
      hideInSearch: true,
      ellipsis: true,
      copyable: true,
      tip: '过长会自动收缩',
    },
    {
      title: '描述',
      dataIndex: 'description',
      hideInSearch: true,
      ellipsis: true,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      hideInSearch: true,
      valueType: 'dateTime',
      width: 150
    },
    {
      title: '修改时间',
      dataIndex: 'updateTime',
      hideInSearch: true,
      valueType: 'dateTime',
      width: 150
    },
    {
      title: '状态',
      dataIndex: 'onlineStatus',
      valueEnum: {
        0: {
          text: '待上线',
          status: 'warning'
        },
        1: {
          text: '已上线',
          status: 'Processing'
        }
      },
      width: 100
    },
    {
      title: '操作',
      width: 150,
      valueType: 'option',
      render: (text, record, _, action) => [
        record.onlineStatus ? <a onClick={() => { offline(record) }}>下线</a> : <a onClick={() => { online(record) }}>上线</a>,
        <a
          onClick={() => {
            setCurrentRow(record);
            setUpdateModalVisible(true)
          }}
        >
          编辑
        </a>,
        <TableDropdown
        key="actionGroup"
        onSelect={async (item) => 
          {
            if (item == 'history') {
              setmodifyLogsModalVisible(true)
              const result = await queryModifyLogs(record)
              if (result.success) {
                setModifyLogs(result.data);
              }
            }

            if (item == 'delete') {
              delConfig(record);
            }
          }
        }
        menus={[
          { key: 'history', name: '历史' },
          { key: 'delete', name: '删除' },
        ]}
      />
      ]
    },
  ];
  return (
    <PageContainer header={{ title: appName }}>
      <ProTable
        actionRef={actionRef}
        rowKey="id"
        options={
          false
        }
        columns={columns}
        search={{
          labelWidth: 'auto',
        }}
        request={(params, sorter, filter) => queryConfigs(appId,params)}
        toolBarRender={() => [
          <Button key="button" icon={<PlusOutlined />} type="primary" onClick={() => { setCreateModalVisible(true); }}>
            新建
          </Button>,
          <Button key="button" type="primary" hidden={selectedRowsState.length == 0} onClick={()=>{onlineSome(selectedRowsState)}}>
            上线
        </Button>,
          <Button key="button" type="primary" danger hidden={selectedRowsState.length == 0} onClick={()=>{offlineSome(selectedRowsState)}}>
            下线
       </Button>,
          <Button key="button" type="primary">
            从json文件导入
          </Button>
        ]}
        rowSelection={{
          onChange: (_, selectedRows) => {
            setSelectedRows(selectedRows);
          },
        }}
      />
      <ModalForm
        formRef={addFormRef}
        title="新建配置"
        visible={createModalVisible}
        onVisibleChange={setCreateModalVisible}
        onFinish={
          async (value) => {
            const success = await handleAdd(value as ConfigListItem);
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
          initialValue={appName}
          rules={[
            {
            },
          ]}
          readonly={true}
          label="应用"
          name="appName"
        />
        <ProFormText
          initialValue={appId}
          rules={[
            {
            },
          ]}
          readonly={true}
          label="appId"
          name="appId"
          hidden={true}
        />
        <ProFormText
          rules={[
            {
            },
          ]}
          label="组"
          name="group"
        />
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label="键"
          name="key"
        />
        <ProFormTextArea
          rules={[
            {
              required: true,
            },
          ]}
          label="值"
          name="value"
          fieldProps={
            {
              autoSize: {
                minRows: 3, maxRows: 8
              }
            }
          }
        />
        <ProFormTextArea
          rules={[
            {
            },
          ]}
          label="描述"
          name="description"
        />
      </ModalForm>
      {
        updateModalVisible &&
        <UpdateForm
          appId={appId}
          appName={appName}
          value={currentRow}
          setValue={setCurrentRow}
          updateModalVisible={updateModalVisible}
          onCancel={
            () => {
              setCurrentRow(undefined);
              setUpdateModalVisible(false);
              console.log('set currentrow undefined');
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
          } />
      }

      <Drawer title="版本历史" visible={modifyLogsModalVisible} width="400" onClose={() => { setmodifyLogsModalVisible(false); setModifyLogs([]); }} >
        <List
          className={styles.history}
          header={false}
          itemLayout="horizontal"
          dataSource={modifyLogs}
          renderItem={(item, index) => (
            <List.Item className={styles.listitem} actions={index ? [<a className={styles.rollback} onClick={ ()=>{rollback(item)} }>回滚</a>] : []} >
              <List.Item.Meta
                title={

                  <div>
                    <Text style={{marginRight:'20px'}}>{moment(item.modifyTime).format('YYYY-MM-DD HH:mm:ss')}</Text>
                  &nbsp;
                  {
                      index ? null : <Tag color="blue">当前版本</Tag>
                    }
                  </div>
                }
                description={
                  <div>
                    <div>组：{item.group}</div>
                    <div>键：{item.key}</div>
                    <div>值：{item.value}</div>
                  </div>
                }
              />
            </List.Item>
          )}
        />
      </Drawer>
    </PageContainer>
  );
}
export default configs;
