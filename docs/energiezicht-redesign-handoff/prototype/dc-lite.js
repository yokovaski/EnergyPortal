/* dc-lite: tiny standalone renderer for the EnergieZicht .dc.html mockups.
   Supports {{path}} holes, <sc-if>, <sc-for>, onX="{{fn}}" handlers and
   class Component extends DCLogic { renderVals() } with setState.
   Only meant for viewing the reference prototypes, not for production. */
(function () {
  var VOID = { input: 1, img: 1, br: 1, meta: 1, link: 1, hr: 1 };
  var srcEl = document.querySelector('template#dc-src');
  var scriptEl = document.querySelector('script[data-dc-script]');
  var propsDecl = JSON.parse(scriptEl.getAttribute('data-props') || '{}');
  var props = {};
  Object.keys(propsDecl).forEach(function (k) { if (k[0] !== '$' && propsDecl[k] && 'default' in propsDecl[k]) props[k] = propsDecl[k]['default']; });
  var q = new URLSearchParams(location.search);
  q.forEach(function (v, k) { props[k] = v === 'true' ? true : v === 'false' ? false : (v !== '' && !isNaN(+v) ? +v : v); });
  if (window.DC_PROPS) Object.assign(props, window.DC_PROPS);

  function DCLogic(p) { this.props = p; this.state = undefined; }
  DCLogic.prototype.setState = function (patch) { this.state = Object.assign({}, this.state || {}, typeof patch === 'function' ? patch(this.state || {}) : patch); render(); };
  DCLogic.prototype.forceUpdate = function () { render(); };
  var Component = new Function('DCLogic', scriptEl.textContent + '\n;return Component;')(DCLogic);
  var inst = new Component(props);
  inst.props = props;

  var HOLE = /\{\{\s*([^}]+?)\s*\}\}/g;
  function lookup(path, scope) {
    path = path.trim();
    if (path === 'true') return true; if (path === 'false') return false; if (path === 'null') return null;
    if (/^-?\d+(\.\d+)?$/.test(path)) return +path;
    var parts = path.split('.'), v = scope;
    for (var i = 0; i < parts.length; i++) { if (v == null) return undefined; v = v[parts[i]]; }
    return v;
  }
  function interp(str, scope) {
    var whole = str.match(/^\s*\{\{\s*([^}]+?)\s*\}\}\s*$/);
    if (whole) return { raw: true, v: lookup(whole[1], scope) };
    return { raw: false, v: str.replace(HOLE, function (_, p) { var x = lookup(p, scope); return x == null ? '' : String(x); }) };
  }
  var BOOL_ATTR = { disabled: 1, checked: 1, readonly: 1, selected: 1, hidden: 1 };
  function build(node, scope, out) {
    if (node.nodeType === 3) { out.appendChild(document.createTextNode(node.nodeValue.replace(HOLE, function (_, p) { var x = lookup(p, scope); return x == null ? '' : String(x); }))); return; }
    if (node.nodeType !== 1) return;
    var tag = node.localName;
    if (tag === 'helmet') return;
    if (tag === 'sc-if') { if (lookup(node.getAttribute('value').replace(/[{}]/g, ''), scope)) kids(node, scope, out); return; }
    if (tag === 'sc-for') {
      var list = lookup(node.getAttribute('list').replace(/[{}]/g, ''), scope) || [];
      var as = node.getAttribute('as') || 'item';
      list.forEach(function (item, i) { var s = Object.create(scope); s[as] = item; s.$index = i; kids(node, s, out); });
      return;
    }
    var el = node.namespaceURI === 'http://www.w3.org/2000/svg' ? document.createElementNS(node.namespaceURI, tag) : document.createElement(tag);
    for (var i = 0; i < node.attributes.length; i++) {
      var a = node.attributes[i], name = a.name, val = a.value;
      if (name.indexOf('hint-') === 0) continue;
      if (/^on[a-z]+$/.test(name)) { var fn = interp(val, scope).v; if (typeof fn === 'function') el.addEventListener(name.slice(2), fn); continue; }
      if (val.indexOf('{{') < 0) { el.setAttribute(name, val); continue; }
      var r = interp(val, scope);
      if (BOOL_ATTR[name] && r.raw) { if (r.v) el.setAttribute(name, ''); continue; }
      if (r.v === undefined || r.v === null) continue;
      el.setAttribute(name, r.raw && typeof r.v === 'boolean' ? String(r.v) : String(r.v));
    }
    if (tag === 'input' && el.hasAttribute('value')) el.value = el.getAttribute('value');
    if (!VOID[tag]) kids(node, scope, el);
    out.appendChild(el);
  }
  function kids(node, scope, out) { var c = (node.content || node).childNodes; for (var i = 0; i < c.length; i++) build(c[i], scope, out); }

  var root = document.getElementById('dc-root');
  function render() {
    var vals = inst.renderVals();
    var frag = document.createDocumentFragment();
    kids(srcEl.content.querySelector('x-dc') || srcEl.content, vals, frag);
    root.innerHTML = '';
    root.appendChild(frag);
    document.querySelectorAll('select option[selected]').forEach(function (o) { o.parentNode.value = o.value; });
  }
  render();
  window.__dcRendered = true;
})();
