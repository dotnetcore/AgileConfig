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
import JsonImport from './comps/JsonImport';
import { useIntl } from 'react-intl';
import { getIntl, getLocale } from '@/.umi/plugin-locale/localeExports';

const { confirm } = Modal;

const handleOnline = async (fields: ConfigListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'publishing'}));
  try {
    const result = await onlineConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id:'publish_success'}));
    } else {
      message.error(intl.formatMessage({id:'publish_fail'}));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id:'publish_fail'}));
    return false;
  }
};

const handleOnlineSome = async (configs: ConfigListItem[]) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'publishing'}));
  try {
    const result = await onlineSomeConfigs(configs);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id:'publish_success'}));
    } else {
      message.error(intl.formatMessage({id:'publish_fail'}));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id:'publish_fail'}));
    return false;
  }
};

const handleOfflineSome = async (configs: ConfigListItem[]) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'offlining'}));
  try {
    const result = await offlineSomeConfigs(configs);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id:'offline_success'}));
    } else {
      message.error(intl.formatMessage({id:'offline_fail'}));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id:'offline_fail'}));
    return false;
  }
};

const handleOffline = async (fields: ConfigListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'offlining'}));
  try {
    const result = await offlineConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id:'offline_success'}));
    } else {
      message.error(intl.formatMessage({id:'offline_fail'}));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id:'offline_fail'}));
    return false;
  }
};

const handleDel = async (fields: ConfigListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'deleting'}));
  try {
    const result = await delConfig({ ...fields });
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
const handleAdd = async (fields: ConfigListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'saving'}));
  try {
    const result = await addConfig({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id:'save_success'}));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id:'save_fail'}));
    return false;
  }
};
const handleEdit = async (config: ConfigListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'saving'}));
  try {
    const result = await editConfig({ ...config });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id:'save_success'}));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id:'save_fail'}));
    return false;
  }
};
const handleRollback = async (config: ConfigModifyLog) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id: 'rollbacking'}));
  try {
    const result = await rollback({ ...config });
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id: 'rollback_success'}));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id: 'rollback_fail'}));
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
  const [jsonImportFormModalVisible, setjsonImportFormModalVisible] = useState<boolean>(false);
  const [currentRow, setCurrentRow] = useState<ConfigListItem>();
  const [selectedRowsState, setSelectedRows] = useState<ConfigListItem[]>([]);
  const [modifyLogs, setModifyLogs] = useState<ConfigModifyLog[]>([]);
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  const intl = useIntl();
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
    const confirmMsg = intl.formatMessage({id:'pages.config.confirm_publish'});
    confirm({
      content: confirmMsg + `【${config.key}】？`,
      onOk: async () => {
        const result = await handleOnline(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const onlineSome = (configs: ConfigListItem[]) => {
    const warningMsg = intl.formatMessage({id:'pages.config.waitpublish_at_least_one'});
    const waitPublishConfigs = configs.filter(x=>x.onlineStatus === 0);
    if (!waitPublishConfigs.length) {
      message.warning(warningMsg);
      return;
    }
    confirm({
      content: intl.formatMessage({id:'pages.config.confirm_publish_some'}),
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
    const warningMsg = intl.formatMessage({id:'pages.config.online_at_least_one'});
    const waitPublishConfigs = configs.filter(x=>x.onlineStatus === 1);
    if (!waitPublishConfigs.length) {
      message.warning(warningMsg);
      return;
    }
    confirm({
      content: intl.formatMessage({id:'pages.config.confirm_offline_some'}),
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
    const confirmMsg = intl.formatMessage({id:'pages.config.confirm_offline'});
    confirm({
      content: confirmMsg + `【${config.key}】？`,
      onOk: async () => {
        const result = await handleOffline(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const delConfig = (config: ConfigListItem) => {
    const confirmMsg = intl.formatMessage({id:'pages.config.confirm_delete'});
    confirm({
      content: confirmMsg + `【${config.key}】？`,
      onOk: async () => {
        const result = await handleDel(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const rollback = (config: ConfigModifyLog) => {
    const confirmMsg = intl.formatMessage({id:'pages.config.confirm_rollback'});
    confirm({
      content: confirmMsg + `【${moment(config.modifyTime).format('YYYY-MM-DD HH:mm:ss')}】？`,
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
      title: intl.formatMessage({id:'pages.configs.table.cols.g'}),
      dataIndex: 'group',
    },
    {
      title: intl.formatMessage({id:'pages.configs.table.cols.k'}),
      dataIndex: 'key',
      copyable: true
    },
    {
      title: intl.formatMessage({id:'pages.configs.table.cols.v'}),
      dataIndex: 'value',
      hideInSearch: true,
      ellipsis: true,
      copyable: true,
      tip: intl.formatMessage({id:'pages.configs.table.cols.v.tip'})
    },
    {
      title: intl.formatMessage({id:'pages.configs.table.cols.desc'}),
      dataIndex: 'description',
      hideInSearch: true,
      ellipsis: true,
    },
    {
      title: intl.formatMessage({id:'pages.configs.table.cols.create_time'}),
      dataIndex: 'createTime',
      hideInSearch: true,
      valueType: 'dateTime',
      width: 150
    },
    {
      title: intl.formatMessage({id:'pages.configs.table.cols.status'}),
      dataIndex: 'onlineStatus',
      valueEnum: {
        0: {
          text: intl.formatMessage({id:'pages.configs.table.cols.status.0'}),
          status: 'warning'
        },
        1: {
          text: intl.formatMessage({id:'pages.configs.table.cols.status.1'}),
          status: 'Processing'
        }
      },
      width: 120
    },
    {
      title: intl.formatMessage({id:'pages.configs.table.cols.action'}),
      width: 150,
      valueType: 'option',
      render: (text, record, _, action) => [
        record.onlineStatus ? <a onClick={() => { offline(record) }}>
          {
            intl.formatMessage({
              id: 'pages.configs.table.cols.action.offline'
            })
          }
        </a> : <a onClick={() => { online(record) }}>
          {
            intl.formatMessage({
              id: 'pages.configs.table.cols.action.publish'
            })
          }
        </a>,
        <a
          onClick={() => {
            setCurrentRow(record);
            setUpdateModalVisible(true)
          }}
        >
          {
            intl.formatMessage({
              id: 'pages.configs.table.cols.action.edit'
            })
          }
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
          { key: 'history', name: intl.formatMessage({
            id: 'pages.configs.table.cols.action.history'
          }) },
          { key: 'delete', name: intl.formatMessage({
            id: 'pages.configs.table.cols.action.delete'
          }) },
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
            {
              intl.formatMessage({
                id: 'pages.configs.table.cols.action.add'
              })
            }
          </Button>,
          <Button key="button" type="primary" hidden={selectedRowsState.length == 0} onClick={()=>{onlineSome(selectedRowsState)}}>
            {
              intl.formatMessage({
                id: 'pages.configs.table.cols.action.publish'
              })
            }
        </Button>,
          <Button key="button" type="primary" danger hidden={selectedRowsState.length == 0} onClick={()=>{offlineSome(selectedRowsState)}}>
            {
              intl.formatMessage({
                id: 'pages.configs.table.cols.action.offline'
              })
            }
       </Button>,
          <Button key="button" type="primary" onClick={()=>{ setjsonImportFormModalVisible(true) }}>
            {
              intl.formatMessage({
                id: 'pages.configs.table.cols.action.importfromjosnfile'
              })
            }
          </Button>
        ]}
        rowSelection={{
          onChange: (_, selectedRows) => {
            setSelectedRows(selectedRows);
          },
        }}
      />
      {
        jsonImportFormModalVisible&&
        <JsonImport
          onSaveSuccess={
            ()=>{
              setjsonImportFormModalVisible(false);
              actionRef.current?.reload();
            }
          }
        onCancel={
          ()=>{
            setjsonImportFormModalVisible(false);
          }
        }
          appId={appId}
          appName={appName}
          jsonImportModalVisible={jsonImportFormModalVisible}> 
          
        </JsonImport>
      }
     
      <ModalForm
        formRef={addFormRef}
        title={intl.formatMessage({id:'pages.configs.from.add.title'})}
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
          label={intl.formatMessage({id:'pages.configs.from.add.app'})}
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
          label={intl.formatMessage({id:'pages.configs.table.cols.g'})}
          name="group"
        />
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label={intl.formatMessage({id:'pages.configs.table.cols.k'})}
          name="key"
        />
        <ProFormTextArea
          rules={[
            {
              required: true,
            },
          ]}
          label={intl.formatMessage({id:'pages.configs.table.cols.v'})}
          name="value"
          fieldProps={
            {
              autoSize: {
                minRows: 3, maxRows: 12
              }
            }
          }
        />
        <ProFormTextArea
          rules={[
            {
            },
          ]}
          label={intl.formatMessage({id:'pages.configs.table.cols.desc'})}
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

      <Drawer title={intl.formatMessage({id:'pages.config.history.title'})} visible={modifyLogsModalVisible} width="400" onClose={() => { setmodifyLogsModalVisible(false); setModifyLogs([]); }} >
        <List
          className={styles.history}
          header={false}
          itemLayout="horizontal"
          dataSource={modifyLogs}
          renderItem={(item, index) => (
            <List.Item className={styles.listitem} actions={index ? [<a className={styles.rollback} onClick={ ()=>{rollback(item)} }>
              {
                intl.formatMessage({id:'pages.config.history.rollback'})
              }
            </a>] : []} >
              <List.Item.Meta
                title={

                  <div>
                    <Text style={{marginRight:'20px'}}>{moment(item.modifyTime).format('YYYY-MM-DD HH:mm:ss')}</Text>
                  &nbsp;
                    {
                       index ? null : <Tag color="blue">
                         {
                          intl.formatMessage({id:'pages.config.history.current'})
                         }
                       </Tag>
                    }
                  </div>
                }
                description={
                  <div>
                    <div>
                      {
                          intl.formatMessage({id:'pages.configs.table.cols.g'})
                      }
                      ：{item.group}</div>
                    <div>
                      {
                          intl.formatMessage({id:'pages.configs.table.cols.k'})
                      }
                      ：{item.key}</div>
                    <div>
                      {
                          intl.formatMessage({id:'pages.configs.table.cols.v'})
                      }：{item.value}</div>
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
