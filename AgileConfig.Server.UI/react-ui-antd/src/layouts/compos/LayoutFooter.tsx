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
          <a href="https://github.com/kklldog/AgileConfig" style={{color:'#bfbfbf'}}><GithubOutlined/> </a>
          &nbsp; Powered by .netcore3.1 ant-design-pro4
        </div>
      )
}

export default LayoutFooter;