import ReactDOM from 'react-dom/client'
import React from 'react'
//import {GoTop} from '@/components/go-top'
import './index.scss' 

import $ from 'jquery'

// const goTopElement = document.getElementById('go-top')
// if (goTopElement) {
//   //ReactDOM.render(<GoTop/>, goTopElement)
//   const root = ReactDOM.createRoot(goTopElement)
//   root.render(<GoTop/>)
// }

const goTopElement2 = $('#test-link').get(0)//document.getElementById('test-link')
if (goTopElement2) {
  //ReactDOM.render(<GoTop/>, goTopElement)
  const root = ReactDOM.createRoot(goTopElement2)
  root.render(<span className={'red-text'} title={'这是一个链接'}>{'链接'}</span>)
}

 
console.debug('async fun2', location.pathname)

// 普通文章或者markdown阅读页面加载特定JS
if (location.pathname.startsWith('/blog/articles/') || location.pathname.startsWith('/articles/')) {
  // const module = await import('./pages/article/read-page')
  // module.default()
  import('./pages/article/read-page').then(module => module.default())
}

