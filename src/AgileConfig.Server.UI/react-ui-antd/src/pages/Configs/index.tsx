import {  DeleteOutlined, DownOutlined, PlusOutlined, RollbackOutlined, VerticalAlignTopOutlined } from '@ant-design/icons';
import { ModalForm, ProFormText, ProFormTextArea } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Badge, Button, Drawer, Dropdown, FormInstance, Input, List, Menu, message, Modal, Radio, Space, Tag } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import UpdateForm from './comps/updateForm';
import { ConfigListItem, PublishDetialConfig } from './data';
import { queryConfigs, delConfig,delConfigs, addConfig, editConfig, queryConfigPublishedHistory, getWaitPublishStatus, publish, cancelEdit, exportJson, cancelSomeEdit } from './service';
import Text from 'antd/lib/typography/Text';
import moment from 'moment';
import styles from './index.less';
import JsonImport from './comps/JsonImport';
import { useIntl } from 'react-intl';
import { getIntl, getLocale } from '@/.umi/plugin-locale/localeExports';
import { checkUserPermission } from '@/components/Authorized/AuthorizedElement';
import { RequireFunction } from '@/utils/permission';
import functionKeys from '@/models/functionKeys';
import VersionHistory from './comps/versionHistory';
import { getFunctions } from '@/utils/authority';
import EnvSync from './comps/EnvSync';
import JsonEditor from './comps/JsonEditor';
import TextEditor from './comps/TextEditor';
import { getEnvList } from '@/utils/system';
import { saveVisitApp } from '@/utils/latestVisitApps';

const { TextArea } = Input;
const { confirm } = Modal;

const handlePublish = async (appId: string, ids:string[], log:string, env:string) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'pages.configs.publishing'}));
  try {
    const result = await publish(appId, ids, log, env);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({id:'pages.configs.publish_success'}));
    } else {
      message.error(intl.formatMessage({id:'pages.configs.publish_fail'}));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id:'pages.configs.publish_fail'}));
    return false;
  }
};

const handleDel = async (fields: ConfigListItem, env:string) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'deleting'}));
  try {
    const result = await delConfig({ ...fields }, env);
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
const handleDelSome = async (configs: ConfigListItem[], env: string):Promise<boolean> => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'deleting'}));
  try {
    const result = await delConfigs(configs, env);
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
const handleCancelEditSome = async (configs: ConfigListItem[], env: string):Promise<boolean> => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'pages.configs.canceling'
  }));
  try {
    const result = await cancelSomeEdit(configs.map(x=>x.id), env);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id: 'pages.configs.cancel_success'
      }));
    } else {
      message.error(intl.formatMessage({
        id: 'pages.configs.cancel_fail'
      }));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id: 'pages.configs.cancel_fail'
    }));
    return false;
  }
};
const handleAdd = async (fields: ConfigListItem, env:string) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'saving'}));
  try {
    const result = await addConfig({ ...fields }, env);
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
const handleEdit = async (config: ConfigListItem, env:string) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'saving'}));
  try {
    const result = await editConfig({ ...config }, env);
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


const handleCancelEdit = async (id: string, env:string) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id: 'pages.configs.canceling'
  }));
  try {
    const result = await cancelEdit(id, env);
    hide();
    const success = result.success;
    if (success) {
      message.success(intl.formatMessage({
        id: 'pages.configs.cancel_success'
      }));
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id: 'pages.configs.cancel_fail'
    }));
    return false;
  }
}

const handleExportJson = async (appId: string, env:string) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'pages.configs.exporting'}));
  try {
    const file = await exportJson(appId, env);
    hide();
    var fileURL = URL.createObjectURL(file);
    var a = document.createElement('a');
    a.href = fileURL;
    a.target = '_blank';
    a.download = appId+'.json';
    document.body.appendChild(a);
    a.click();
    URL.revokeObjectURL(a.href);
    document.body.removeChild(a);
    return true;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({id:'pages.configs.export_fail'}));
    return false;
  }
}

const configs: React.FC = (props: any) => {
  const appId = props.match.params.app_id;
  const appName = props.match.params.app_name;
  useEffect(()=>{
    saveVisitApp(appId,appName);
  },[]);
  const [createModalVisible, setCreateModalVisible] = useState<boolean>(false);
  const [updateModalVisible, setUpdateModalVisible] = useState<boolean>(false);
  const [modifyLogsModalVisible, setmodifyLogsModalVisible] = useState<boolean>(false);
  const [jsonImportFormModalVisible, setjsonImportFormModalVisible] = useState<boolean>(false);
  const [versionHistoryFormModalVisible, setVersionHistoryFormModalVisible] = useState<boolean>(false);
  const [jsonEditorVisible, setJsonEditorVisible] = useState<boolean>(false);
  const [textEditorVisible, setTextEditorVisible] = useState<boolean>(false);
  const [EnvSyncModalVisible, setEnvSyncModalVisible] = useState<boolean>(false);
  const [currentRow, setCurrentRow] = useState<ConfigListItem>();
  const [selectedRowsState, setSelectedRows] = useState<ConfigListItem[]>([]);
  const [configPublishedHistory, setModifyLogs] = useState<PublishDetialConfig[]>([]);
  const [waitPublishStatus, setWaitPublishStatus] = useState<{
    addCount: number,
    editCount: number,
    deleteCount: number
  }>({
    addCount: 0,
    editCount: 0,
    deleteCount: 0
  });
  const envList = getEnvList();
  const [currentEnv, setCurrentEnv] = useState<string>(envList[0]);
  const [tableData, setTableData] = useState<ConfigListItem[]>([]);
  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
  let _publishLog:string = '';
  const intl = useIntl();
  useEffect(() => {
    getWaitPublishStatus(appId, currentEnv).then(x => {
      console.log('WaitPublishStatus ', x);
      if (x.success) {
        setWaitPublishStatus(x.data);
      }      
    })
  }, [tableData]);

  const publish = (appId: string) => {
    const rows = selectedRowsState.filter(x=>x.editStatus !== 10).map(x=>x.id);
    const isPublishAll = rows.length === 0;
    const msg = isPublishAll ? intl.formatMessage({id:'pages.configs.table.cols.action.publish'}) : intl.formatMessage({id:'pages.configs.confirm_publish_current'});
    _publishLog = '';
    confirm({
      content: <div>
        {
          intl.formatMessage({id:'pages.configs.confirm_publish_current'}) + `【${msg}】` + intl.formatMessage({id:'pages.configs.confirm_publish_wait_items'})
        }
        <br />
        <br />
        <div>
         <TextArea autoSize placeholder={intl.formatMessage({
           id: 'pages.configs.publish_log_placeholder'
         })} maxLength={50} showCount={true}
          onChange={(e)=>{
            _publishLog = e.target.value;
          }}
         >
         </TextArea>
        </div>
      </div>,
      onOk: async () => {

        const result = await handlePublish(appId, rows, _publishLog, currentEnv);
        if (result && actionRef.current) {
          if (actionRef.current?.clearSelected){
            actionRef.current?.clearSelected();
          }
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
        const result = await handleDel(config, currentEnv);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }

  const editStatusEnums = {
    0: intl.formatMessage({id:'pages.configs.table.cols.status.0'}),
    1: intl.formatMessage({id:'pages.configs.table.cols.action.edit'}),
    2: intl.formatMessage({id:'pages.configs.table.cols.action.delete'}),
    10: intl.formatMessage({id:'pages.configs.table.cols.status.1'})
  }
  const editStatusColors = {
    0: 'blue',
    1: 'gold',
    2: 'red',
    10: ''
  }
  const columns: ProColumns<ConfigListItem>[] = [
 
    {
      title: intl.formatMessage({id:'pages.configs.table.cols.g'}),
      dataIndex: 'group',
      copyable: true,
      sorter: true,
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
      width: 150,
      sorter: true,
    },
    {
      title: intl.formatMessage({id:'pages.configs.edit_status'}),
      dataIndex: 'editStatus',
      search: false,
      render: (_, record) => (
         <Tag color={editStatusColors[record.editStatus as keyof typeof editStatusColors]}>
           {
              editStatusEnums[record.editStatus as keyof typeof editStatusEnums]
           }
         </Tag>
      ),
    },
    {
      title: intl.formatMessage({id:'pages.configs.publish_status'}),
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
        <RequireFunction fn={functionKeys.Config_Edit} key="edit" fallback={null} appId={appId}>
          <a
            onClick={() => {
              setCurrentRow(record);
              setUpdateModalVisible(true);
            }}
          >
            {intl.formatMessage({ id: 'pages.configs.table.cols.action.edit' })}
          </a>
        </RequireFunction>,
        <RequireFunction fn={functionKeys.Config_Delete} key="delete" fallback={null} appId={appId}>
          <a
            onClick={() => {
              delConfig(record);
            }}
          >
            {intl.formatMessage({ id: 'pages.configs.table.cols.action.delete' })}
          </a>
        </RequireFunction>,
        <RequireFunction fn={functionKeys.Config_Delete} key="dropdown" fallback={null} appId={appId}>
          <TableDropdown
            key="actionGroup"
            onSelect={async (item) => {
              if (item == 'history') {
                setCurrentRow(record);
                setmodifyLogsModalVisible(true);
                const result = await queryConfigPublishedHistory(record, currentEnv);
                if (result.success) {
                  setModifyLogs(result.data);
                }
              }
              if (item == 'cancelEdit') {
                confirm({
                  content:
                    intl.formatMessage({ id: 'pages.configs.confirm_cancel_edit' }) +
                    `【${record.group ? record.group : ''}${record.group ? ':' : ''}${record.key}】` +
                    intl.formatMessage({ id: 'pages.configs.confirm_cancel_edit_suffix' }),
                  onOk: async () => {
                    const result = await handleCancelEdit(record.id, currentEnv);
                    if (result) {
                      actionRef.current?.reload();
                    }
                  },
                });
              }
            }}
            menus={
              record.editStatus === 10
                ? [
                    {
                      key: 'history',
                      name: intl.formatMessage({ id: 'pages.configs.table.cols.action.history' }),
                    },
                  ]
                : [
                    {
                      key: 'cancelEdit',
                      name: intl.formatMessage({ id: 'pages.configs.table.cols.action.cancel_edit' }),
                    },
                    {
                      key: 'history',
                      name: intl.formatMessage({ id: 'pages.configs.table.cols.action.history' }),
                    },
                  ]
            }
          />
        </RequireFunction>,
      ]
    },
  ];
  return (
    <PageContainer pageHeaderRender={
        ()=>{
          return <div style={{padding:20, display:'flex', justifyContent:'space-between', alignItems:'center'}}>
            <span className="ant-page-header-heading-title">
              {
                appName
              }
            </span>
            <div>
            <Radio.Group defaultValue={currentEnv} buttonStyle="solid" size="small" onChange={
              (e)=>{
                console.log(e.target.value);
                setCurrentEnv(e.target.value);
                actionRef.current?.reload(true);
              }}>
                {
                  envList.map(e=>{
                    return <Radio.Button value={e} key={e}>{e}</Radio.Button>
                  })
                }
            </Radio.Group>
            </div>
          </div>
        }
      }>
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
        request={(params, sorter, filter) => {
          let sortField = 'createTime';
          let ascOrDesc = 'descend';
          for (const key in sorter) {
            sortField = key;
            const val = sorter[key];
            if (val) {
              ascOrDesc = val;
            }
          }
          console.log(sortField, ascOrDesc);
          return queryConfigs(appId, currentEnv, { sortField, ascOrDesc, ...params })
        }}
        headerTitle= {
          <Space size="middle">
              <Badge count={waitPublishStatus.addCount} size="small" offset={[-5, 0]}>
                <Tag color="blue" hidden={waitPublishStatus.addCount===0}>{intl.formatMessage({id:'pages.configs.table.cols.status.0'})}</Tag>
              </Badge>
              <Badge count={waitPublishStatus.editCount} size="small" offset={[-5, 0]}>
                <Tag color="gold" hidden={waitPublishStatus.editCount===0}>{intl.formatMessage({id:'pages.configs.table.cols.action.edit'})}</Tag>
              </Badge>
              <Badge count={waitPublishStatus.deleteCount} size="small" offset={[-5, 0]}>
                <Tag color="red" hidden={waitPublishStatus.deleteCount===0}>{intl.formatMessage({id:'pages.configs.table.cols.action.delete'})}</Tag>
              </Badge>
          </Space>
        }
        toolBarRender={() => [
          <RequireFunction fn={functionKeys.Config_Add} key="add" fallback={null} appId={appId}>
            <Button type="primary" icon={<PlusOutlined />} onClick={() => { setCreateModalVisible(true); }}>
              {intl.formatMessage({ id: 'pages.configs.table.cols.action.add' })}
            </Button>
          </RequireFunction>
          ,
          <RequireFunction fn={functionKeys.Config_Publish} key="publish" fallback={null} appId={appId}>
            <Button
              icon={<VerticalAlignTopOutlined />}
              type="primary"
              className="success"
              hidden={
                waitPublishStatus.addCount + waitPublishStatus.editCount + waitPublishStatus.deleteCount === 0
              }
              onClick={() => {
                publish(appId);
              }}
            >
              {selectedRowsState.filter((x) => x.editStatus !== 10).length > 0
                ? intl.formatMessage({ id: 'pages.configs.publish_selected' })
                : intl.formatMessage({ id: 'pages.configs.publish_all' })}
            </Button>
          </RequireFunction>
          ,
          <RequireFunction fn={functionKeys.Config_Publish} key="cancelEdit" fallback={null} appId={appId}>
            {selectedRowsState.filter((x) => x.editStatus !== 10).length > 0 ? (
              <Button
                type="primary"
                icon={<RollbackOutlined />}
                className="warn"
                onClick={() => {
                  confirm({
                    content: intl.formatMessage({ id: 'pages.configs.confirm_cancel_selected_edit' }),
                    onOk: async () => {
                      const result = await handleCancelEditSome(
                        selectedRowsState.filter((x) => x.editStatus !== 10),
                        currentEnv
                      );
                      if (result) {
                        if (actionRef.current?.clearSelected) {
                          actionRef.current?.clearSelected();
                        }
                        actionRef.current?.reload();
                      }
                    },
                  });
                }}
              >
                {intl.formatMessage({ id: 'pages.configs.table.cols.action.cancel_edit' })}
              </Button>
            ) : null}
          </RequireFunction>
        ,
        <RequireFunction fn={functionKeys.Config_Edit} key="deleteSelected" fallback={null} appId={appId}>
          {selectedRowsState.length > 0 ? (
            <Button
              type="primary"
              icon={<DeleteOutlined />}
              className="danger"
              onClick={() => {
                confirm({
                  content: intl.formatMessage({ id: 'pages.configs.confirm_delete_selected' }),
                  onOk: async () => {
                    const result = await handleDelSome(selectedRowsState, currentEnv);
                    if (result) {
                      if (actionRef.current?.clearSelected) {
                        actionRef.current?.clearSelected();
                      }
                      actionRef.current?.reload();
                    }
                  },
                });
              }}
            >
              {intl.formatMessage({ id: 'pages.configs.table.cols.action.delete' })}
            </Button>
          ) : null}
        </RequireFunction>
        ,
         <RequireFunction fn={functionKeys.Config_Edit} key="editJson" fallback={null} appId={appId}>
          <Button onClick={() => { setJsonEditorVisible(true); }}>
            {intl.formatMessage({ id: 'pages.configs.table.cols.action.edit_json' })}
          </Button>
        </RequireFunction>
          ,
         <RequireFunction fn={functionKeys.Config_Edit} key="editText" fallback={null} appId={appId}>
          <Button onClick={() => { setTextEditorVisible(true); }}>
            {intl.formatMessage({ id: 'pages.configs.table.cols.action.edit_text' })}
          </Button>
        </RequireFunction>
          ,
          <Dropdown overlay={
            <Menu >
              <Menu.Item hidden={!checkUserPermission(getFunctions(),functionKeys.Config_Publish,appId)} key="history" onClick={()=>{ setVersionHistoryFormModalVisible(true) }}>
               {intl.formatMessage({id:'pages.config.history.title'})}
              </Menu.Item>
              <Menu.Item hidden={!checkUserPermission(getFunctions(),functionKeys.Config_Add,appId)} key="syncEnv" onClick={()=>{ setEnvSyncModalVisible(true) }}>
                {intl.formatMessage({id: 'pages.configs.env_sync'})}
              </Menu.Item>
              <Menu.Item hidden={!checkUserPermission(getFunctions(),functionKeys.Config_Add,appId)} key="import" onClick={()=>{ setjsonImportFormModalVisible(true) }}>
                {intl.formatMessage({id:'pages.configs.table.cols.action.importfromjosnfile'})}
              </Menu.Item>
              <Menu.Item key="export" onClick={()=>{handleExportJson(appId, currentEnv)}}>
                {intl.formatMessage({id:'pages.configs.table.cols.action.exportJson'})}
              </Menu.Item>
            </Menu>
          }>
          <Button>
            {intl.formatMessage({id: 'pages.configs.more'})} <DownOutlined />
          </Button>
        </Dropdown>

          // <Button key="6" onClick={()=>{
          //   setTextEditorVisible(true);
          // }}>
          //   Edit TEXT
          // </Button>
        ]}
        rowSelection={{
          onChange: (_, selectedRows) => {
            setSelectedRows(selectedRows);
          },
        }}
        postData={
          (data:ConfigListItem[])=>{
            setTableData(data);
            return data;
          }
        }
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
          jsonImportModalVisible={jsonImportFormModalVisible}
          env={currentEnv}
          > 
        </JsonImport>
      }
      {
        versionHistoryFormModalVisible&&
        <VersionHistory
          onSaveSuccess={
            ()=>{
              setVersionHistoryFormModalVisible(false);
              actionRef.current?.reload();
            }
          }
        onCancel={
          (reload)=>{
            setVersionHistoryFormModalVisible(false);
            if (reload) {
              actionRef.current?.reload();
            }
          }
        }
          env={currentEnv}
          appId={appId}
          appName={appName}
          versionHistoryModalVisible ={versionHistoryFormModalVisible}> 
        </VersionHistory>
      }
      {
        EnvSyncModalVisible&&
        <EnvSync currentEnv={currentEnv}
                appId={appId} 
                ModalVisible={EnvSyncModalVisible}
                onSaveSuccess={
                  ()=>{
                    setEnvSyncModalVisible(false);
                    actionRef.current?.reload();
                  }
                }
                onCancel={
                  ()=>{
                    setEnvSyncModalVisible(false);
                  }
                } >
        </EnvSync>
      }
      <ModalForm
        modalProps= {
          {
            maskClosable:false
          }
        }
        formRef={addFormRef}
        title={intl.formatMessage({id:'pages.configs.from.title.add'})}
        visible={createModalVisible}
        onVisibleChange={setCreateModalVisible}
        onFinish={
          async (value) => {
            const success = await handleAdd(value as ConfigListItem, currentEnv);
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
              required: false,
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
              const success = await handleEdit(value, currentEnv);
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
      {
        jsonEditorVisible && 
        <JsonEditor 
        appId={appId}
        appName={appName}
        env={currentEnv}
        ModalVisible={jsonEditorVisible}
        onCancel={
          () => {
            setJsonEditorVisible(false)
          }
        }
        onSaveSuccess={
          async () => {
            setJsonEditorVisible(false)
            if (actionRef.current) {
              actionRef.current.reload();
            }
          }
        }
        >
        </JsonEditor>
      }
      {
        textEditorVisible && 
        <TextEditor 
        appId={appId}
        appName={appName}
        env={currentEnv}
        ModalVisible={textEditorVisible}
        onCancel={
          () => {
            setTextEditorVisible(false)
          }
        }
        onSaveSuccess={
          async () => {
            setTextEditorVisible(false)
            if (actionRef.current) {
              actionRef.current.reload();
            }
          }
        }
        >
        </TextEditor>
      }

      <Drawer title={intl.formatMessage({id:'pages.config.history.title'})} visible={modifyLogsModalVisible} width="400" onClose={() => { setmodifyLogsModalVisible(false); setModifyLogs([]); }} >
        <List
          className={styles.history}
          header={false}
          itemLayout="horizontal"
          dataSource={configPublishedHistory}
          renderItem={(item, index) => (
            <List.Item className={styles.listitem}  >
              <List.Item.Meta
                title={

                  <div>
                    <Text style={{marginRight:'20px'}}>{moment(item.timelineNode?.publishTime).format('YYYY-MM-DD HH:mm:ss')}</Text>
                  </div>
                }
                description={
                  <div>
                    <div>
                      {
                          intl.formatMessage({id:'pages.configs.table.cols.g'})
                      }
                      ：{item.config.group}</div>
                    <div>
                      {
                          intl.formatMessage({id:'pages.configs.table.cols.k'})
                      }
                      ：{item.config.key}</div>
                    <div>
                      {
                          intl.formatMessage({id:'pages.configs.table.cols.v'})
                      }：{item.config.value}</div>
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
