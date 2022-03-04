import { ExclamationCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { ModalForm,  ProFormDependency, ProFormSelect, ProFormSwitch, ProFormText } from '@ant-design/pro-form';
import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ActionType, ProColumns } from '@ant-design/pro-table';
import { Button, Checkbox, Divider, FormInstance, Input, message, Modal, Space, Switch, Tag } from 'antd';
import React, { useState, useRef, useEffect } from 'react';
import { getIntl, getLocale, Link, useIntl} from 'umi';
import UpdateForm from './comps/updateForm';
import { AppListItem, AppListParams, AppListResult, UserAppAuth } from './data';
import { addApp, editApp, delApp, queryApps, inheritancedApps,enableOrdisableApp, saveAppAuth, getAppGroups } from './service';
import { adminUsers } from '@/pages/User/service';
import UserAuth from './comps/userAuth';
import AuthorizedEle from '@/components/Authorized/AuthorizedElement';
import functionKeys from '@/models/functionKeys';
import { current } from '@/services/user';
import { getUserInfo, setAuthority, setFunctions } from '@/utils/authority';

const { confirm } = Modal;

const fetchSystemInfo = async () => {
   const result = await current();
   setAuthority(result.currentUser.currentAuthority);
   setFunctions(result.currentUser.currentFunctions)
}

const handleAdd = async (fields: AppListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id:'saving'
  }));
  try {
    const result = await addApp({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      fetchSystemInfo();
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
const handleEdit = async (app: AppListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id:'saving'
  }));
  try {
    const result = await editApp({ ...app });
    hide();
    const success = result.success;
    if (success) {
      fetchSystemInfo();
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
const handleDel = async (fields: AppListItem) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id:'deleting'
  }));
  try {
    const result = await delApp({ ...fields });
    hide();
    const success = result.success;
    if (success) {
      fetchSystemInfo();
      message.success(intl.formatMessage({
        id:'delete_success'
      }));
    } else {
      message.error(intl.formatMessage({
        id:'delete_fail'
      }));
    }
    return success;
  } catch (error) {
    hide();
    message.error(intl.formatMessage({
      id:'delete_fail'
    }));
    return false;
  }
};
const handleUserAppAuth = async (model: UserAppAuth) => {
  const intl = getIntl(getLocale());
  const hide = message.loading(intl.formatMessage({
    id:'saving'
  }));
  try {
    const result = await saveAppAuth({ ...model });
    hide();
    const success = result.success;
    if (success) {
      fetchSystemInfo();
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

const appList: React.FC = (props) => {

  const actionRef = useRef<ActionType>();
  const addFormRef = useRef<FormInstance>();
 
  const intl = useIntl();

  const [createModalVisible, setCreateModalVisible] = useState<boolean>(false);
  const [updateModalVisible, setUpdateModalVisible] = useState<boolean>(false);
  const [userAuthModalVisible, setUserAuthModalVisible] = useState<boolean>(false);
  const [currentRow, setCurrentRow] = useState<AppListItem>();
  const [dataSource, setDataSource] = useState<AppListResult>();
  const [appGroups, setAppGroups] = useState<{label:string, value:string}[]>([]);
  const [newAppGroupName, setNewAppGroupName] = useState<string>('');
  const [appGroupsEnums, setAppGroupsEnums] = useState<{}>({});
  const [tableGrouped, setTableGrouped] = useState<boolean>(false);

  useEffect(()=>{
    getAppGroups().then(x=>{
      if (x.success) {
        const groups:{label:string, value:string}[] = [];
        const groupEnums = {};
        x.data.forEach((i: any)=>{
          groups.push({
            label:i,
            value: i
          });
          groupEnums[i]={text:i};
        })
        setAppGroups(groups);
        setAppGroupsEnums(groupEnums);
      } 
    })
  },[dataSource]);

  const handleQuery = async (params: AppListParams) => {
    const result = await queryApps(params);
    setDataSource(result);
    return result;
  }
  const handleEnabledChange =async (checked:boolean, entity:AppListItem) => {
    const result = await enableOrdisableApp(entity.id);
    const qiyong = intl.formatMessage({
      id: 'enabled.1'
    });
    const jinyong = intl.formatMessage({
      id: 'enabled.0'
    });
    const success = intl.formatMessage({
      id: 'success'
    });
    const failed = intl.formatMessage({
      id: 'failed'
    });
    if (result.success) {
      const msg = (checked?qiyong:jinyong) + success;
      message.success(msg);
      if (dataSource) {
        const app = dataSource?.data?.find(x=>x.id === entity.id);
        if (app) {
          app.enabled = checked;
        }
        setDataSource({...dataSource});
      }
    }
    else{
      message.error((checked?qiyong:jinyong) + failed)
    }
  }
  const columns: ProColumns<AppListItem>[] = [
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.appname'
      }),
      dataIndex: 'name',
      sorter: true,
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.appid'
      }),
      dataIndex: 'id',
      copyable: true,
      sorter: true,
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.secret'
      }),
      dataIndex: 'secret',
      valueType: 'password',
      hideInSearch: true,
      copyable: true,
    },
    {
      title: '应用组',
      sorter: true,
      valueType: 'select',
      dataIndex: 'group',
      valueEnum: appGroupsEnums
      // request:async ()=>{
      //   const groups = await getAppGroups();
      //   const arr:{label:string, value:string}[] = [];
      //   groups.data.forEach( (x: string)=>{
      //     arr.push({
      //       value: x,
      //       label: x,
      //     });
      //   });
      //   return arr;
      // }
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.create_time'
      }),
      dataIndex: 'createTime',
      valueType: 'dateTime',
      hideInSearch: true,
      sorter: true,
    },
    {
      title: '管理员',
      dataIndex: 'appAdminName',
      hideInSearch: true,
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.public'
      }),
      dataIndex: 'inheritanced',
      hideInSearch: true,
      valueEnum: {
        false: {
          text:  intl.formatMessage({
            id:'pages.app.inheritanced.false'
          }),
          status: 'default'
        },
        true: {
          text:  intl.formatMessage({
            id:'pages.app.inheritanced.true'
          }),
          status: 'success'
        }
      }
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.link'
      }),
      dataIndex: 'inheritancedApps',
      search: false,
      renderFormItem: (_, { defaultRender }) => {
        return defaultRender(_);
      },
      render: (_, record) => (
        <Space>
          {record.inheritancedAppNames?.map((name:string) => (
            <Tag color="blue" key={name}>
              {name}
            </Tag>
          ))}
        </Space>
      ),
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.enabled'
      }),
      dataIndex: 'enabled',
      render: (dom, entity) => {
        return <AuthorizedEle appId={entity.id} judgeKey={functionKeys.App_Edit} noMatch={
                  <Switch checked={entity.enabled} size="small" />
                }>
                <Switch checked={entity.enabled} size="small" onChange={
                (e)=>{
                  handleEnabledChange(e, entity);
                }
                }/>
        </AuthorizedEle>
      },
      hideInSearch: true
    },
    {
      title: intl.formatMessage({
        id:'pages.app.table.cols.action'
      }),
      valueType: 'option',
      render: (text, record, _, action) => [
        <Link key="0"
        to={
          {
            pathname:'/app/config/' + record.id + '/' + record.name,
          }
        }
      >{intl.formatMessage({
        id:'pages.app.table.cols.action.configs'
      })}</Link>,
        <AuthorizedEle key="1" appId={record.id}  judgeKey={functionKeys.App_Edit}>
          <a
            onClick={() => {
              setUpdateModalVisible(true);
              setCurrentRow(record);
            }}
          >
            {
              intl.formatMessage({
                id:'pages.app.table.cols.action.edit'
              })
            }
          </a>
        </AuthorizedEle>
        ,
          <a key="2"
            onClick={()=>{
              setUserAuthModalVisible(true);
              setCurrentRow(record);
            }}>
            授权
          </a>
        ,
        <AuthorizedEle key="3" appId={record.id}  judgeKey={functionKeys.App_Delete}>
          <Button type="link" danger 
            onClick={() => {
              const msg = intl.formatMessage({
                id:'pages.app.delete_msg'
              }) + `【${record.name}】?`;
              confirm({
                icon: <ExclamationCircleOutlined />,
                content: msg,
                async onOk() {
                  console.log('delete app ' + record.name);
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
                id:'pages.app.table.cols.action.delete'
              })
            }
          </Button>
        </AuthorizedEle>
        
      ]
    }
  ];
  return (
    <PageContainer>
      <ProTable
        actionRef={actionRef}
        options={
          false
        }
        search={{
          labelWidth: 'auto',
        }}
        
        rowKey={row=>row.id}
        columns={columns}
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
          return handleQuery({ tableGrouped, sortField, ascOrDesc, ...params })
        } }
        headerTitle = {
          <Checkbox onChange={(e)=>{ 
            setTableGrouped(e.target.checked);
            actionRef.current?.reload();
          }}>分组聚合</Checkbox>
        }
        toolBarRender={() => {
          return [
            <AuthorizedEle key="0" judgeKey={functionKeys.App_Add} > 
               <Button key="button" icon={<PlusOutlined />} type="primary" onClick={() => { setCreateModalVisible(true) }}>
                 {
                   intl.formatMessage({
                     id:'pages.app.table.cols.action.add'
                   })
                 }
               </Button>
            </AuthorizedEle>
         ]
        }}
        //dataSource={dataSource}
      />
      <ModalForm
        formRef={addFormRef}
        title={
          intl.formatMessage({
            id: 'pages.app.form.title.add'
          })
        }
        visible={createModalVisible}
        onVisibleChange={setCreateModalVisible}
        onFinish={
          async (value) => {
            const success = await handleAdd(value as AppListItem);
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
          label={
            intl.formatMessage({
              id: 'pages.app.form.name'
            })
          }
          name="name"
        />
        <ProFormText
          rules={[
            {
              required: true,
            },
          ]}
          label={
            intl.formatMessage({
              id: 'pages.app.form.id'
            })
          }
          name="id"
        />
        <ProFormText.Password
          rules={[
            {
            },
          ]}
          label={
            intl.formatMessage({
              id: 'pages.app.form.secret'
            })
          }
          name="secret"
        />
        <ProFormSelect
                placeholder="应用所属的组"
                  label="应用组"
                  name="group"
                  options={appGroups}
                  fieldProps={{
                    dropdownRender: (menu) => (
                      <div>
                      {menu}
                      <Divider style={{ margin: '4px 0' }} />
                        <div style={{ display: 'flex', flexWrap: 'nowrap', padding: 8 }}>
                          <Input placeholder="输入组名" style={{ flex: 'auto' }} value={newAppGroupName} onChange={(e)=>{ setNewAppGroupName(e.target.value) }} />
                          <a
                            style={{ flex: 'none', padding: '8px', display: 'block', cursor: 'pointer' }}
                            onClick={()=>{
                              if(newAppGroupName){
                                setAppGroups([...appGroups, {
                                  label: newAppGroupName,
                                  value: newAppGroupName
                                }]);
                                setNewAppGroupName('');
                              }
                            }}
                          >
                            <PlusOutlined /> 
                          </a>
                        </div>
                      </div>
                    )
                  }}
        ></ProFormSelect>
        <ProFormSwitch tooltip={
          intl.formatMessage({
            id: 'pages.app.form.public.tooltip'
          })
        } label={
           intl.formatMessage({
            id: 'pages.app.form.public'
          })
        } name="inheritanced" checkedChildren={true} unCheckedChildren={false}>
        </ProFormSwitch>
        <ProFormDependency name={
          ["inheritanced"]
        }>
          {
            (e) => {
              return !e.inheritanced ?
                <ProFormSelect
                  tooltip={
                    intl.formatMessage({
                      id: 'pages.app.form.connected.tooltip'
                    })
                  }
                  label={
                    intl.formatMessage({
                      id: 'pages.app.form.connected'
                    })
                  }
                  name="inheritancedApps"
                  mode="multiple" 
                  request={async () => {
                    const result = await inheritancedApps('');
                    return result.data.map( (x: { name: string, id: string })=> {
                      console.log(x);
                      return { label:x.name, value:x.id};
                    });
                  }}
                ></ProFormSelect> : null
            }
          }
        </ProFormDependency>
        <ProFormSelect
                rules={[
                  {
                    required: true,
                  },
                ]}
                  initialValue={getUserInfo().userid}
                  label="管理员"
                  name="appAdmin"
                  request={async () => {
                    const result = await adminUsers();
                    return result.data.map( (x: { userName: string, id: string, team:string })=> {
                      console.log(x);
                      return { label:x.userName + ' - ' + (x.team?x.team:''), value:x.id};
                    });
                  }}
        ></ProFormSelect>
        
        <ProFormSwitch label={
          intl.formatMessage({
            id: 'pages.app.form.enabled'
          })
        } name="enabled" initialValue={true} checkedChildren={true} unCheckedChildren={false}>
        </ProFormSwitch>
      </ModalForm>
      {
        updateModalVisible &&
        <UpdateForm
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
          }/>
      }
      {
        userAuthModalVisible && 
        <UserAuth
          value = {currentRow}
          userAuthModalVisible={userAuthModalVisible}
          onCancel={
            () => {
              setUserAuthModalVisible(false);
            }
          }
          onSubmit={
            async (value) => {
              const success = await handleUserAppAuth(value);
              if (success) {
                setUserAuthModalVisible(false);
              }
            }
          }
        >
        </UserAuth>
      }
    </PageContainer>
  );
}
export default appList;
