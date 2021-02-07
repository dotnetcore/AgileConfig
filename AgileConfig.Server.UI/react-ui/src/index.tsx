import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';

interface SquareProps {
  value : string ,
  click(): void 
}

class Square extends React.Component<SquareProps> {
  constructor (props: SquareProps) {
    super(props);
  }
  render () {
    return (
      <button className="square" onClick={ ()=>{ this.props.click() } }>
        {this.props.value}
      </button>
    );
  }
}

function Square2 (props: SquareProps) {
  return (
    <button className="square" onClick={ ()=>{ props.click() } }>
      { props.value }
    </button>
  );
}

class Board extends React.Component<{},{squares:Array<string>}> {
  constructor (props:any) {
    super(props);
    this.state = {
      squares:[
        '','','',
        '','','',
        '','','',
      ]
    }
  }
  handlerClick (i: number) {
    let sqs = this.state.squares;
    sqs[i] = 'X';
    this.setState({
      squares: sqs
    });
  }
  renderSquare (i: number) {
    return <Square2  value={this.state.squares[i]} click={ ()=>{ this.handlerClick(i) } }/>;
  };
  render() {
    const status = 'Next player: X';

    return (
      <div>
        <div className="status">{status}</div>
        <div className="board-row">
          {this.renderSquare(0)}
          {this.renderSquare(1)}
          {this.renderSquare(2)}
        </div>
        <div className="board-row">
        {this.renderSquare(3)}
          {this.renderSquare(4)}
          {this.renderSquare(5)}
        </div>
        <div className="board-row">
        {this.renderSquare(6)}
          {this.renderSquare(7)}
          {this.renderSquare(8)}
        </div>
      </div>
    );
  }
}

function Game() {
  return (
    <div className="game">
    <div className="game-board">
      <Board />
    </div>
    <div className="game-info">
      <div>{/* status */}</div>
      <ol>{/* TODO */}</ol>
    </div>
    </div>
  );
}

ReactDOM.render(
  <React.StrictMode>
    <Game />
  </React.StrictMode>,
  document.getElementById('root')
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
