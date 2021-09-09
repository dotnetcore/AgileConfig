import { CiCircleOutlined, DeleteOutlined, PlusOutlined, RollbackOutlined, VerticalAlignTopOutlined } from '@ant-design/icons';
import { ModalForm, ProFormText, ProFormTextArea } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns, TableDropdown } from '@ant-design/pro-table';
import { Badge, Button, Drawer, FormInstance, Input, List, message, Modal, Space, Tag } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { queryApps } from '../Apps/service';
import UpdateForm from './comps/updateForm';
import { ConfigListItem, PublishDetialConfig } from './data';
import { queryConfigs, delConfig,delConfigs, addConfig, editConfig, queryConfigPublishedHistory, getWaitPublishStatus, publish, cancelEdit, exportJson, cancelSomeEdit } from './service';
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

const { TextArea } = Input;
const { confirm } = Modal;

const handlePublish = async (appId: string, log:string) => {
  const hide = message.loading('正在发布');
  try {
    const result = await publish(appId, log);
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
const handleDelSome = async (configs: ConfigListItem[]):Promise<boolean> => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({id:'deleting'}));
  try {
    const result = await delConfigs(configs);
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
const handleCancelEditSome = async (configs: ConfigListItem[]):Promise<boolean> => {
  const hide = message.loading('正在撤销');
  try {
    const result = await cancelSomeEdit(configs.map(x=>x.id));
    hide();
    const success = result.success;
    if (success) {
      message.success('撤销成功');
    } else {
      message.error('撤销失败');
    }
    return success;
  } catch (error) {
    hide();
    message.error('撤销失败');
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


const handleCancelEdit = async (id: string) => {
  const hide = message.loading('正在撤销');
  try {
    const result = await cancelEdit(id);
    hide();
    const success = result.success;
    if (success) {
      message.success('撤销成功！');
    } else {
      message.error(result.message);
    }
    return success;
  } catch (error) {
    hide();
    message.error('撤销失败！');
    return false;
  }
}

const handleExportJson = async (appId: string) => {
  const hide = message.loading('正在导出');
  try {
    const file = await exportJson(appId);
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
    message.error('导出失败！');
    return false;
  }
}

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
  let _publishLog:string = '';
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
    _publishLog = '';
    confirm({
      content: <div>
        {
          '确定发布当前所有待发布的配置项吗？'
        }
        <br />
        <br />
        <div>
         <TextArea autoSize placeholder="请填写发布日志" maxLength={50} showCount={true}
          onChange={(e)=>{
            _publishLog = e.target.value;
          }}
         >
         </TextArea>
        </div>
      </div>,
      onOk: async () => {
        const result = await handlePublish(appId, _publishLog);
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

                  if (item == 'cancelEdit') {
                    confirm({
                      content:`确定撤销对配置【${record.group?record.group:''}${record.group?':':''}${record.key}】的编辑吗？`,
                      onOk: async ()=>{
                        const result = await handleCancelEdit(record.id);
                        if (result) {
                          actionRef.current?.reload();
                        }
                      }
                    })
                  }
                }
              }
              menus={
                record.editStatus === 10 ?
                [
                  { key: 'history', name: intl.formatMessage({
                    id: 'pages.configs.table.cols.action.history'
                  }) }
                ]:
                [
                  { key: 'cancelEdit', name: '撤销编辑' },
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
          return queryConfigs(appId,{ sortField, ascOrDesc, ...params })
        }}
        headerTitle= {
          <Space size="middle">
              <Badge count={waitPublishStatus.addCount} size="small" offset={[-5, 0]}>
                <Tag color="blue" hidden={waitPublishStatus.addCount===0}>新增</Tag>
              </Badge>
              <Badge count={waitPublishStatus.editCount} size="small" offset={[-5, 0]}>
                <Tag color="gold" hidden={waitPublishStatus.editCount===0}>编辑</Tag>
              </Badge>
              <Badge count={waitPublishStatus.deleteCount} size="small" offset={[-5, 0]}>
                <Tag color="red" hidden={waitPublishStatus.deleteCount===0}>删除</Tag>
              </Badge>
          </Space>
        }
        toolBarRender={() => [
          <AuthorizedEle key="0" judgeKey={functionKeys.Config_Add} appId={appId}>
            <Button key="button" type="primary" icon={<PlusOutlined />}  onClick={() => { setCreateModalVisible(true); }}>
            {
              intl.formatMessage({
                id: 'pages.configs.table.cols.action.add'
              })
            }
            </Button>
          </AuthorizedEle>
          ,
          <AuthorizedEle key="2" judgeKey={functionKeys.Config_Publish} appId={appId} >
            <Button key="button" icon={<VerticalAlignTopOutlined />} type="primary" className="success"
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
          <AuthorizedEle key="5" judgeKey={functionKeys.Config_Edit} appId={appId} >
            {
              selectedRowsState.filter(x=>x.editStatus !== 10).length > 0 ?
              <Button key="button"  type="primary" icon={<RollbackOutlined />}
                      className="warn"
                      onClick={
                        ()=>{
                          confirm({
                            content:`确定撤销选中配置项的编辑状态吗？`,
                            onOk: async ()=>{
                              const result = await handleCancelEditSome(selectedRowsState.filter(x=>x.editStatus !== 10))
                              if (result) {
                                if (actionRef.current?.clearSelected){
                                  actionRef.current?.clearSelected();
                                }
                                actionRef.current?.reload();
                              }
                            }
                          })
                        }
                      }
                    >
                撤销编辑
              </Button>
              :
              <></>
            }
            
          </AuthorizedEle>
        ,
        <AuthorizedEle key="6" judgeKey={functionKeys.Config_Edit} appId={appId} >
          {
            selectedRowsState.length > 0 ?
            <Button key="button"  type="primary" icon={<DeleteOutlined />}
                    className="danger"
                    onClick={
                      ()=>{
                        confirm({
                          content:`确定删除选中的配置项吗？`,
                          onOk: async ()=>{
                            const result = await handleDelSome(selectedRowsState)
                            if (result) {
                              if (actionRef.current?.clearSelected){
                                actionRef.current?.clearSelected();
                              }
                              actionRef.current?.reload();
                            }
                          }
                        })
                      }
                    }
                    >
                删除
            </Button>:<></>
          }
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
          <Button key="4" onClick={()=>{
            handleExportJson(appId)
          }}>
            导出
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
          (reload)=>{
            setVersionHistoryFormModalVisible(false);
            if (reload) {
              actionRef.current?.reload();
            }
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
