import { useEffect } from "react";
import { summaryProps } from "./summary";
import * as echarts from 'echarts';
import styles from '../index.less';

const NodeStatiCharts: React.FC<summaryProps> = (props)=> {
    useEffect(() => {
      const el = document.getElementById('nodeClientsChart');
        if (el) {
          const myChart = echarts.init(el);
          const option = {
            legend: {
              data: ['客户端数量']
            },
            tooltip: {
              trigger: 'axis',
              axisPointer: {            // 坐标轴指示器，坐标轴触发有效
                  type: 'shadow'        // 默认为直线，可选为：'line' | 'shadow'
              }
            },
            title: {
            },
            xAxis: {
                type: 'category',
                data: props.nodeStatiInfo?.chartCategorys??[]
            },
            yAxis: {
                type: 'value'
            },
            series: [{
                name:'客户端数量',
                data: props.nodeStatiInfo?.chartValues??[],
                type: 'bar'
            }]
          };
          myChart.setOption(option);
        }
    }, [props.nodeStatiInfo]);
    return(
      <div className={styles.panel}>
        <div id="nodeClientsChart" className={styles.node_clients} >
        </div>
      </div>
  
    );
  }

  export default NodeStatiCharts;