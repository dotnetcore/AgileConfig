import { PageContainer } from '@ant-design/pro-layout';
import ProTable, { ProColumns } from '@ant-design/pro-table';
import React, { useState, useRef, useEffect } from 'react';
import { FormattedMessage } from 'umi';
import { TableListItem } from '../TableList/data';
import { queryLogs } from './service';
import {queryApps} from '../Apps/service'

const logs:React.FC = () =>  {
  const [appEnums, setAppEnums] = useState<any>();
  const getAppEnums = async () =>
  {
    const result = await queryApps({})
    const obj = {};
    result.data.forEach((x)=>{
      obj[x.id] = {
        text: x.name
      }
    });

    return obj;
  }
  const getAppsForSelect = async () =>
  {
    const result = await queryApps({})
    const arr:any[] = [];
    result.data.forEach((x)=>{
       arr.push({
         value: x.id,
         label: x.name
       });
    });

    return arr;
  }
  useEffect(()=>{
    getAppEnums().then(x=> {
      console.log('app enums ', x);
      setAppEnums({...x});
    });
  }, []);
 
  const columns: ProColumns<TableListItem>[] = [
    {
      title: '应用',
      dataIndex: 'appId',
      valueType: 'select',
      valueEnum: appEnums,
    },
    {
      title: '类型',
      dataIndex: 'logType',
      valueType: 'select',
      valueEnum: {
        '': { text: '全部', status: '' },
        0: {
          text: '普通',
        },
        1: {
          text: '警告',
        }
      },
    },
    {
      title: '时间',
      dataIndex: 'logTime',
      valueType: 'dateTime',
      hideInSearch: true,
    },
    {
      title: '时间',
      dataIndex: 'logTime',
      valueType: 'dateRange',
      hideInTable: true,
      search: {
        transform: (value) => {
          return {
            startTime: value[0],
            endTime: value[1],
          };
        },
      }
    },
    {
      title: '内容',
      dataIndex: 'logTxt',
      hideInSearch: true
    },
  ];
  return (
    <PageContainer>
      <ProTable<TableListItem>       
      options={
        false
      }    
      search={{
        labelWidth: 'auto',
      }}                                                                         
        rowKey="id"
        columns = {columns}
        request = { (params, sorter, filter) => queryLogs({ ...params}) }
      />
    </PageContainer>
  );
}
export default logs;
