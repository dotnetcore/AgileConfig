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
  const handleSave = async (kvText: string|undefined, appId:string, env:string) => {
    const hide = message.loading('正在保存...');
    try {
      const result = await saveKvList(appId, env, kvText?kvText:"");
      hide();
      const success = result.success;
      if (success) {
        message.success('保存成功！');
      } else {
        message.error(result.message? result.message: '保存失败！');
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
          footer={
            <div style={{display:'flex', justifyContent:'space-between'}}>
              <div style={{
                color:'#999',
                fontSize:'12px',
              }}>
                严格按照 KEY=VALUE 格式编辑，每行一个配置
              </div>
              <div>
              <Button onClick={ 
                  ()=>{
                    props.onCancel();
                  }
                }>取消</Button>
                <Button type="primary" onClick={
                  async ()=> {
                    const saveResult = await handleSave(kvText, props.appId, props.env);
                    if (saveResult) {
                      props.onSaveSuccess();
                    }
                  }
                }>保存</Button>
              </div>
            </div>
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