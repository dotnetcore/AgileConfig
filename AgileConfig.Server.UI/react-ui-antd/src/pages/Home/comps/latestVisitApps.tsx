import { Card } from 'antd';
import { history } from 'umi';
import { useState } from 'react';
import { getVisitApps } from '@/utils/latestVisitApps';

const LatestVisitApps: React.FC = ()=> {
    const [latestVisitApps, _] = useState<{appId:string,appName:string}[]>(getVisitApps());
    return(
      <Card title="最近访问" hidden={latestVisitApps.length === 0}>
      {
        latestVisitApps.map(x=>  
          <Card.Grid style={{  width: '25%'}} key={x.appId}>
           <div style={{cursor:'pointer'}} onClick={
             ()=>{
                history.push(`/app/config/${x.appId}/${x.appName}`);
             }
           }>
              <p>{x.appId}</p>
              {x.appName}
           </div>
          </Card.Grid>
        )
      }
    </Card>
  
    );
  }

  export default LatestVisitApps;