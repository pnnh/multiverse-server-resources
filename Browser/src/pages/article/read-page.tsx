import Prism from 'prismjs'

export default () => {
  console.debug('read-page 语法高亮')
  // 代码块语法高亮
  const codes = document.getElementsByTagName('code') 
  if (codes) {
    Array.from(codes).forEach(e => {
      if (!(e instanceof HTMLElement)) {
        return
      }
      const code = e.innerText
      const language = e.className
      console.debug('code language: ', language)
      if (language) {
        let html = code
        if (Prism.languages[language]) {
          html = Prism.highlight(code, Prism.languages[language], language)
        }
        e.innerHTML = html
      }
    })
  }
}
