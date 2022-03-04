import { getAppVer } from "@/utils/system";
import { GithubOutlined } from "@ant-design/icons"
import React from "react"

const LayoutFooter : React.FC =()=>{
    return (
        <div style={{
          display:'flex',
          justifyContent: 'center',
          marginBottom: '10px',
          color: '#bfbfbf'
        }}>
          v
          { getAppVer()}
          &nbsp;&nbsp;  
          <a title={'agileconfig ' + getAppVer()} href="https://github.com/kklldog/AgileConfig" style={{color:'#bfbfbf'}}><GithubOutlined/> </a>
          &nbsp; Powered by .NET6.0 ant-design-pro4
        </div>
      )
}

export default LayoutFooter;