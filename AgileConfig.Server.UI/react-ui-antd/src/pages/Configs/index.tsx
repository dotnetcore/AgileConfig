import { PlusOutlined, VerticalAlignTopOutlined } from '@ant-design/icons';
import { ModalForm, ProFormText, ProFormTextArea } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Button, Drawer, FormInstance, List, message, Modal, Tag } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { queryApps } from '../Apps/service';
import UpdateForm from './comps/updateForm';
import { ConfigListItem, PublishDetial, PublishDetialConfig } from './data';
import { queryConfigs, delConfig, addConfig, editConfig, queryConfigPublishedHistory,rollback, getWaitPublishStatus, publish } from './service';
import Text from 'antd/lib/typography/Text';
import moment from 'moment';
import styles from './index.less';
import JsonImport from './comps/JsonImport';
import { useIntl } from 'react-intl';
import { getIntl, getLocale } from '@/.umi/plugin-locale/localeExports';
import AuthorizedEle, { checkUserPermission } from '@/components/Authorized/AuthorizedElement';
import functionKeys from '@/models/functionKeys';
import { getFunctions } from '@/utils/authority';
import VersionHistory from './comps/versionHistory';

const { confirm } = Modal;

const handlePublish = async (appId: string) => {
  const hide = message.loading('正在发布');
  try {
    const result = await publish(appId);
    hide();
    const success = result.success;
    if (success) {
      message.success('发布成功！');
    } else {
      message.error('发布失败！');
    }
    return success;
  } catch (error) {
    hide();
    message.error('发布失败！');
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
const handleRollback = async (config: PublishDetial) => {
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
  const [versionHistoryFormModalVisible, setVersionHistoryFormModalVisible] = useState<boolean>(false);
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
  const [tableData, setTableData] = useState<ConfigListItem[]>([]);
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
  useEffect(() => {
    getWaitPublishStatus(appId).then(x => {
      console.log('WaitPublishStatus ', x);
      if (x.success) {
        setWaitPublishStatus(x.data);
      }      
    })
  }, [tableData]);

  const publish = (appId: string) => {
    confirm({
      content: '确定发布当前待发布的配置项吗？',
      onOk: async () => {
        const result = await handlePublish(appId);
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
        const result = await handleDel(config);
        if (result) {
          actionRef.current?.reload();
        }
      }
    });
  }
  const rollback = (config: PublishDetialConfig) => {
    const confirmMsg = intl.formatMessage({id:'pages.config.confirm_rollback'});
    confirm({
      content: confirmMsg + `【${moment(config.timelineNode?.publishTime).format('YYYY-MM-DD HH:mm:ss')}】？`,
      onOk: async () => {
        const result = await handleRollback(config.config);
        if (result) {
          setmodifyLogsModalVisible(false);
          actionRef.current?.reload();
        }
      }
    });
  }

  const editStatusEnums = {
    0: '新增',
    1: '编辑',
    2: '删除',
    10: '已提交'
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
      copyable: true
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
      title: '编辑状态',
      dataIndex: 'editStatus',
      search: false,
      render: (_, record) => (
         <Tag color={editStatusColors[record.editStatus]}>
           {
              editStatusEnums[record.editStatus]
           }
         </Tag>
      ),
    },
    {
      title: '发布状态',
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
        <AuthorizedEle key="1" judgeKey={functionKeys.Config_Edit} appId={record.appId}>
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
          </a>
        </AuthorizedEle>
        ,
        <AuthorizedEle key="2" judgeKey={functionKeys.Config_Delete} appId={record.appId}>
          <a
            onClick={() => {
              delConfig(record);
            }}
          >
            {
              intl.formatMessage({
                id: 'pages.configs.table.cols.action.delete'
              })
            }
          </a>
        </AuthorizedEle>
        ,
        <AuthorizedEle key="3" judgeKey={functionKeys.Config_Delete} appId={record.appId}>
          <TableDropdown
              key="actionGroup"
              onSelect={async (item) => 
                {
                  if (item == 'history') {
                    setCurrentRow(record);
                    setmodifyLogsModalVisible(true)
                    const result = await queryConfigPublishedHistory(record)
                    if (result.success) {
                      setModifyLogs(result.data);
                    }
                  }
                }
              }
              menus={
                [
                  { key: 'history', name: intl.formatMessage({
                    id: 'pages.configs.table.cols.action.history'
                  }) }
                ]
              }
            />
        </AuthorizedEle>
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
        headerTitle= {`add:${waitPublishStatus.addCount} edit:${waitPublishStatus.editCount} delete:${waitPublishStatus.deleteCount}`}
        toolBarRender={() => [
          <AuthorizedEle key="0" judgeKey={functionKeys.Config_Add} appId={appId}>
            <Button key="button" icon={<PlusOutlined />}  onClick={() => { setCreateModalVisible(true); }}>
            {
              intl.formatMessage({
                id: 'pages.configs.table.cols.action.add'
              })
            }
            </Button>
          </AuthorizedEle>
          ,
          <AuthorizedEle key="2" judgeKey={functionKeys.Config_Publish} appId={appId} >
            <Button key="button" icon={<VerticalAlignTopOutlined />} type="primary" 
                    hidden={(waitPublishStatus.addCount + waitPublishStatus.editCount + waitPublishStatus.deleteCount) === 0} 
                    onClick={()=>{publish(appId)}}>
                {
                  intl.formatMessage({
                    id: 'pages.configs.table.cols.action.publish'
                  })
                }
            </Button>
          </AuthorizedEle>
          ,
          <AuthorizedEle key="1" judgeKey={functionKeys.Config_Publish} appId={appId}>
            <Button key="button"  onClick={()=>{ setVersionHistoryFormModalVisible(true) }}>
              {
                '历史版本'
              }
            </Button>
          </AuthorizedEle>
          ,
        <AuthorizedEle key="3" judgeKey={functionKeys.Config_Add} appId={appId}>
          <Button onClick={()=>{ setjsonImportFormModalVisible(true) }}>
            {
              intl.formatMessage({
                id: 'pages.configs.table.cols.action.importfromjosnfile'
              })
            }
          </Button>
        </AuthorizedEle>
         ,
          <Button key="4">
            <a href={'/config/exportjson?appId=' + appId} target="_blank">
              {
                 intl.formatMessage({
                  id: 'pages.configs.table.cols.action.exportJson'
                })
              }
            </a>
          </Button>
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
          jsonImportModalVisible={jsonImportFormModalVisible}> 
          
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
          ()=>{
            setVersionHistoryFormModalVisible(false);
          }
        }
          appId={appId}
          appName={appName}
          versionHistoryModalVisible ={versionHistoryFormModalVisible}> 
          
        </VersionHistory>
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
          dataSource={configPublishedHistory}
          renderItem={(item, index) => (
            <List.Item className={styles.listitem}  >
              <List.Item.Meta
                title={

                  <div>
                    <Text style={{marginRight:'20px'}}>{moment(item.timelineNode?.publishTime).format('YYYY-MM-DD HH:mm:ss')}</Text>
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
