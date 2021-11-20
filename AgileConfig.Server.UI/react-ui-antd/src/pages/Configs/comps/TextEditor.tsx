import { Button, message, Modal, Input } from "antd";
const { TextArea } = Input;
import Editor from "@monaco-editor/react";
import React, { useState, useEffect } from 'react';
import { getConfigsKvList, saveKvList } from "../service";
export type TextEditorProps = {
    appId: string,
    appName: string,
    ModalVisible: boolean;
    env: string,
    onCancel: () => void;
    onSaveSuccess: ()=> void;
  };
  const handleSave = async (kvText: string, appId:string, env:string) => {
    const hide = message.loading('正在保存...');
    try {

      const lines = kvText.split('\r\n');
      const kvs = [];
      for (const line of lines) {
        const arr = line.split('=');
        if (arr.length < 2) {
          continue;
        }
        kvs.push({
          key : arr[0],
          value : arr[1]
        });
      }

      const result = await saveKvList(appId, env, kvs);
      hide();
      const success = result.success;
      if (success) {
        message.success('保存成功！');
      } else {
        message.error('保存失败！');
      }
      return success;
    } catch (error) {
      hide();
      message.error('保存失败！');
      return false;
    }
  };
const TextEditor : React.FC<TextEditorProps> = (props)=>{

  const [kvText, setkvText] = useState<string|undefined>("");

    useEffect(()=>{
      getConfigsKvList(props.appId, props.env).then(res=>{
        if (res.success) {
          const list = res.data.map((x: { value: string; key: string; }) =>  x.key + '=' + x.value);
          setkvText(list.join('\n'));
        }
      });
    },[])

    return (
        <Modal 
          title="按 TEXT 视图编辑"
          okText="保存"
          width={800} 
          visible={props.ModalVisible}
          onCancel={
            ()=>{
              props.onCancel();
            }
          }
          onOk={
            async ()=> {
              if(kvText) {
                const saveResult = await handleSave(kvText, props.appId, props.env);
                if (saveResult) {
                  props.onSaveSuccess();
                }
              }else{
                message.error('键值对文本不能为空。');
              }
            }
          }
          >
         <Editor
              height="500px"
              defaultLanguage="text"
              defaultValue=''
              value={kvText}
              options= { {minimap: { enabled:false } }}
              onChange= { (v, e)=> {
                setkvText(v);
              }}
            />
        </Modal>
    );
}

export default TextEditor;