//! Licensed to the .NET Foundation under one or more agreements.
//! The .NET Foundation licenses this file to you under the MIT license.

var e=!1;const t=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,4,1,96,0,0,3,2,1,0,10,8,1,6,0,6,64,25,11,11])),o=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,15,1,13,0,65,1,253,15,65,2,253,15,253,128,2,11])),n=async()=>WebAssembly.validate(new Uint8Array([0,97,115,109,1,0,0,0,1,5,1,96,0,1,123,3,2,1,0,10,10,1,8,0,65,0,253,15,253,98,11])),r=Symbol.for("wasm promise_control");function i(e,t){let o=null;const n=new Promise((function(n,r){o={isDone:!1,promise:null,resolve:t=>{o.isDone||(o.isDone=!0,n(t),e&&e())},reject:e=>{o.isDone||(o.isDone=!0,r(e),t&&t())}}}));o.promise=n;const i=n;return i[r]=o,{promise:i,promise_control:o}}function s(e){return e[r]}function a(e){e&&function(e){return void 0!==e[r]}(e)||Be(!1,"Promise is not controllable")}const l="__mono_message__",c=["debug","log","trace","warn","info","error"],d="MONO_WASM: ";let u,f,m,g,p,h;function w(e){g=e}function b(e){if(Pe.diagnosticTracing){const t="function"==typeof e?e():e;console.debug(d+t)}}function y(e,...t){console.info(d+e,...t)}function v(e,...t){console.info(e,...t)}function E(e,...t){console.warn(d+e,...t)}function _(e,...t){if(t&&t.length>0&&t[0]&&"object"==typeof t[0]){if(t[0].silent)return;if(t[0].toString)return void console.error(d+e,t[0].toString())}console.error(d+e,...t)}function x(e,t,o){return function(...n){try{let r=n[0];if(void 0===r)r="undefined";else if(null===r)r="null";else if("function"==typeof r)r=r.toString();else if("string"!=typeof r)try{r=JSON.stringify(r)}catch(e){r=r.toString()}t(o?JSON.stringify({method:e,payload:r,arguments:n.slice(1)}):[e+r,...n.slice(1)])}catch(e){m.error(`proxyConsole failed: ${e}`)}}}function j(e,t,o){f=t,g=e,m={...t};const n=`${o}/console`.replace("https://","wss://").replace("http://","ws://");u=new WebSocket(n),u.addEventListener("error",A),u.addEventListener("close",S),function(){for(const e of c)f[e]=x(`console.${e}`,T,!0)}()}function R(e){let t=30;const o=()=>{u?0==u.bufferedAmount||0==t?(e&&v(e),function(){for(const e of c)f[e]=x(`console.${e}`,m.log,!1)}(),u.removeEventListener("error",A),u.removeEventListener("close",S),u.close(1e3,e),u=void 0):(t--,globalThis.setTimeout(o,100)):e&&m&&m.log(e)};o()}function T(e){u&&u.readyState===WebSocket.OPEN?u.send(e):m.log(e)}function A(e){m.error(`[${g}] proxy console websocket error: ${e}`,e)}function S(e){m.debug(`[${g}] proxy console websocket closed: ${e}`,e)}function D(){Pe.preferredIcuAsset=O(Pe.config);let e="invariant"==Pe.config.globalizationMode;if(!e)if(Pe.preferredIcuAsset)Pe.diagnosticTracing&&b("ICU data archive(s) available, disabling invariant mode");else{if("custom"===Pe.config.globalizationMode||"all"===Pe.config.globalizationMode||"sharded"===Pe.config.globalizationMode){const e="invariant globalization mode is inactive and no ICU data archives are available";throw _(`ERROR: ${e}`),new Error(e)}Pe.diagnosticTracing&&b("ICU data archive(s) not available, using invariant globalization mode"),e=!0,Pe.preferredIcuAsset=null}const t="DOTNET_SYSTEM_GLOBALIZATION_INVARIANT",o=Pe.config.environmentVariables;if(void 0===o[t]&&e&&(o[t]="1"),void 0===o.TZ)try{const e=Intl.DateTimeFormat().resolvedOptions().timeZone||null;e&&(o.TZ=e)}catch(e){y("failed to detect timezone, will fallback to UTC")}}function O(e){var t;if((null===(t=e.resources)||void 0===t?void 0:t.icu)&&"invariant"!=e.globalizationMode){const t=e.applicationCulture||(ke?globalThis.navigator&&globalThis.navigator.languages&&globalThis.navigator.languages[0]:Intl.DateTimeFormat().resolvedOptions().locale),o=e.resources.icu;let n=null;if("custom"===e.globalizationMode){if(o.length>=1)return o[0].name}else t&&"all"!==e.globalizationMode?"sharded"===e.globalizationMode&&(n=function(e){const t=e.split("-")[0];return"en"===t||["fr","fr-FR","it","it-IT","de","de-DE","es","es-ES"].includes(e)?"icudt_EFIGS.dat":["zh","ko","ja"].includes(t)?"icudt_CJK.dat":"icudt_no_CJK.dat"}(t)):n="icudt.dat";if(n)for(let e=0;e<o.length;e++){const t=o[e];if(t.virtualPath===n)return t.name}}return e.globalizationMode="invariant",null}(new Date).valueOf();const C=class{constructor(e){this.url=e}toString(){return this.url}};async function k(e,t){try{const o="function"==typeof globalThis.fetch;if(Se){const n=e.startsWith("file://");if(!n&&o)return globalThis.fetch(e,t||{credentials:"same-origin"});p||(h=Ne.require("url"),p=Ne.require("fs")),n&&(e=h.fileURLToPath(e));const r=await p.promises.readFile(e);return{ok:!0,headers:{length:0,get:()=>null},url:e,arrayBuffer:()=>r,json:()=>JSON.parse(r),text:()=>{throw new Error("NotImplementedException")}}}if(o)return globalThis.fetch(e,t||{credentials:"same-origin"});if("function"==typeof read)return{ok:!0,url:e,headers:{length:0,get:()=>null},arrayBuffer:()=>new Uint8Array(read(e,"binary")),json:()=>JSON.parse(read(e,"utf8")),text:()=>read(e,"utf8")}}catch(t){return{ok:!1,url:e,status:500,headers:{length:0,get:()=>null},statusText:"ERR28: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t},text:()=>{throw t}}}throw new Error("No fetch implementation available")}function I(e){return"string"!=typeof e&&Be(!1,"url must be a string"),!M(e)&&0!==e.indexOf("./")&&0!==e.indexOf("../")&&globalThis.URL&&globalThis.document&&globalThis.document.baseURI&&(e=new URL(e,globalThis.document.baseURI).toString()),e}const U=/^[a-zA-Z][a-zA-Z\d+\-.]*?:\/\//,P=/[a-zA-Z]:[\\/]/;function M(e){return Se||Ie?e.startsWith("/")||e.startsWith("\\")||-1!==e.indexOf("///")||P.test(e):U.test(e)}let L,N=0;const $=[],z=[],W=new Map,F={"js-module-threads":!0,"js-module-runtime":!0,"js-module-dotnet":!0,"js-module-native":!0,"js-module-diagnostics":!0},B={...F,"js-module-library-initializer":!0},V={...F,dotnetwasm:!0,heap:!0,manifest:!0},q={...B,manifest:!0},H={...B,dotnetwasm:!0},J={dotnetwasm:!0,symbols:!0},Z={...B,dotnetwasm:!0,symbols:!0},Q={symbols:!0};function G(e){return!("icu"==e.behavior&&e.name!=Pe.preferredIcuAsset)}function K(e,t,o){null!=t||(t=[]),Be(1==t.length,`Expect to have one ${o} asset in resources`);const n=t[0];return n.behavior=o,X(n),e.push(n),n}function X(e){V[e.behavior]&&W.set(e.behavior,e)}function Y(e){Be(V[e],`Unknown single asset behavior ${e}`);const t=W.get(e);if(t&&!t.resolvedUrl)if(t.resolvedUrl=Pe.locateFile(t.name),F[t.behavior]){const e=ge(t);e?("string"!=typeof e&&Be(!1,"loadBootResource response for 'dotnetjs' type should be a URL string"),t.resolvedUrl=e):t.resolvedUrl=ce(t.resolvedUrl,t.behavior)}else if("dotnetwasm"!==t.behavior)throw new Error(`Unknown single asset behavior ${e}`);return t}function ee(e){const t=Y(e);return Be(t,`Single asset for ${e} not found`),t}let te=!1;async function oe(){if(!te){te=!0,Pe.diagnosticTracing&&b("mono_download_assets");try{const e=[],t=[],o=(e,t)=>{!Z[e.behavior]&&G(e)&&Pe.expected_instantiated_assets_count++,!H[e.behavior]&&G(e)&&(Pe.expected_downloaded_assets_count++,t.push(se(e)))};for(const t of $)o(t,e);for(const e of z)o(e,t);Pe.allDownloadsQueued.promise_control.resolve(),Promise.all([...e,...t]).then((()=>{Pe.allDownloadsFinished.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),await Pe.runtimeModuleLoaded.promise;const n=async e=>{const t=await e;if(t.buffer){if(!Z[t.behavior]){t.buffer&&"object"==typeof t.buffer||Be(!1,"asset buffer must be array-like or buffer-like or promise of these"),"string"!=typeof t.resolvedUrl&&Be(!1,"resolvedUrl must be string");const e=t.resolvedUrl,o=await t.buffer,n=new Uint8Array(o);pe(t),await Ue.beforeOnRuntimeInitialized.promise,Ue.instantiate_asset(t,e,n)}}else J[t.behavior]?("symbols"===t.behavior&&(await Ue.instantiate_symbols_asset(t),pe(t)),J[t.behavior]&&++Pe.actual_downloaded_assets_count):(t.isOptional||Be(!1,"Expected asset to have the downloaded buffer"),!H[t.behavior]&&G(t)&&Pe.expected_downloaded_assets_count--,!Z[t.behavior]&&G(t)&&Pe.expected_instantiated_assets_count--)},r=[],i=[];for(const t of e)r.push(n(t));for(const e of t)i.push(n(e));Promise.all(r).then((()=>{Ce||Ue.coreAssetsInMemory.promise_control.resolve()})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e})),Promise.all(i).then((async()=>{Ce||(await Ue.coreAssetsInMemory.promise,Ue.allAssetsInMemory.promise_control.resolve())})).catch((e=>{throw Pe.err("Error in mono_download_assets: "+e),Xe(1,e),e}))}catch(e){throw Pe.err("Error in mono_download_assets: "+e),e}}}let ne=!1;function re(){if(ne)return;ne=!0;const e=Pe.config,t=[];if(e.assets)for(const t of e.assets)"object"!=typeof t&&Be(!1,`asset must be object, it was ${typeof t} : ${t}`),"string"!=typeof t.behavior&&Be(!1,"asset behavior must be known string"),"string"!=typeof t.name&&Be(!1,"asset name must be string"),t.resolvedUrl&&"string"!=typeof t.resolvedUrl&&Be(!1,"asset resolvedUrl could be string"),t.hash&&"string"!=typeof t.hash&&Be(!1,"asset resolvedUrl could be string"),t.pendingDownload&&"object"!=typeof t.pendingDownload&&Be(!1,"asset pendingDownload could be object"),t.isCore?$.push(t):z.push(t),X(t);else if(e.resources){const o=e.resources;o.wasmNative||Be(!1,"resources.wasmNative must be defined"),o.jsModuleNative||Be(!1,"resources.jsModuleNative must be defined"),o.jsModuleRuntime||Be(!1,"resources.jsModuleRuntime must be defined"),K(z,o.wasmNative,"dotnetwasm"),K(t,o.jsModuleNative,"js-module-native"),K(t,o.jsModuleRuntime,"js-module-runtime"),o.jsModuleDiagnostics&&K(t,o.jsModuleDiagnostics,"js-module-diagnostics");const n=(e,t,o)=>{const n=e;n.behavior=t,o?(n.isCore=!0,$.push(n)):z.push(n)};if(o.coreAssembly)for(let e=0;e<o.coreAssembly.length;e++)n(o.coreAssembly[e],"assembly",!0);if(o.assembly)for(let e=0;e<o.assembly.length;e++)n(o.assembly[e],"assembly",!o.coreAssembly);if(0!=e.debugLevel&&Pe.isDebuggingSupported()){if(o.corePdb)for(let e=0;e<o.corePdb.length;e++)n(o.corePdb[e],"pdb",!0);if(o.pdb)for(let e=0;e<o.pdb.length;e++)n(o.pdb[e],"pdb",!o.corePdb)}if(e.loadAllSatelliteResources&&o.satelliteResources)for(const e in o.satelliteResources)for(let t=0;t<o.satelliteResources[e].length;t++){const r=o.satelliteResources[e][t];r.culture=e,n(r,"resource",!o.coreAssembly)}if(o.coreVfs)for(let e=0;e<o.coreVfs.length;e++)n(o.coreVfs[e],"vfs",!0);if(o.vfs)for(let e=0;e<o.vfs.length;e++)n(o.vfs[e],"vfs",!o.coreVfs);const r=O(e);if(r&&o.icu)for(let e=0;e<o.icu.length;e++){const t=o.icu[e];t.name===r&&n(t,"icu",!1)}if(o.wasmSymbols)for(let e=0;e<o.wasmSymbols.length;e++)n(o.wasmSymbols[e],"symbols",!1)}if(e.appsettings)for(let t=0;t<e.appsettings.length;t++){const o=e.appsettings[t],n=he(o);"appsettings.json"!==n&&n!==`appsettings.${e.applicationEnvironment}.json`||z.push({name:o,behavior:"vfs",cache:"no-cache",useCredentials:!0})}e.assets=[...$,...z,...t]}async function ie(e){const t=await se(e);return await t.pendingDownloadInternal.response,t.buffer}async function se(e){try{return await ae(e)}catch(t){if(!Pe.enableDownloadRetry)throw t;if(Ie||Se)throw t;if(e.pendingDownload&&e.pendingDownloadInternal==e.pendingDownload)throw t;if(e.resolvedUrl&&-1!=e.resolvedUrl.indexOf("file://"))throw t;if(t&&404==t.status)throw t;e.pendingDownloadInternal=void 0,await Pe.allDownloadsQueued.promise;try{return Pe.diagnosticTracing&&b(`Retrying download '${e.name}'`),await ae(e)}catch(t){return e.pendingDownloadInternal=void 0,await new Promise((e=>globalThis.setTimeout(e,100))),Pe.diagnosticTracing&&b(`Retrying download (2) '${e.name}' after delay`),await ae(e)}}}async function ae(e){for(;L;)await L.promise;try{++N,N==Pe.maxParallelDownloads&&(Pe.diagnosticTracing&&b("Throttling further parallel downloads"),L=i());const t=await async function(e){if(e.pendingDownload&&(e.pendingDownloadInternal=e.pendingDownload),e.pendingDownloadInternal&&e.pendingDownloadInternal.response)return e.pendingDownloadInternal.response;if(e.buffer){const t=await e.buffer;return e.resolvedUrl||(e.resolvedUrl="undefined://"+e.name),e.pendingDownloadInternal={url:e.resolvedUrl,name:e.name,response:Promise.resolve({ok:!0,arrayBuffer:()=>t,json:()=>JSON.parse(new TextDecoder("utf-8").decode(t)),text:()=>{throw new Error("NotImplementedException")},headers:{get:()=>{}}})},e.pendingDownloadInternal.response}const t=e.loadRemote&&Pe.config.remoteSources?Pe.config.remoteSources:[""];let o;for(let n of t){n=n.trim(),"./"===n&&(n="");const t=le(e,n);e.name===t?Pe.diagnosticTracing&&b(`Attempting to download '${t}'`):Pe.diagnosticTracing&&b(`Attempting to download '${t}' for ${e.name}`);try{e.resolvedUrl=t;const n=fe(e);if(e.pendingDownloadInternal=n,o=await n.response,!o||!o.ok)continue;return o}catch(e){o||(o={ok:!1,url:t,status:0,statusText:""+e});continue}}const n=e.isOptional||e.name.match(/\.pdb$/)&&Pe.config.ignorePdbLoadErrors;if(o||Be(!1,`Response undefined ${e.name}`),!n){const t=new Error(`download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`);throw t.status=o.status,t}y(`optional download '${o.url}' for ${e.name} failed ${o.status} ${o.statusText}`)}(e);return t?(J[e.behavior]||(e.buffer=await t.arrayBuffer(),++Pe.actual_downloaded_assets_count),e):e}finally{if(--N,L&&N==Pe.maxParallelDownloads-1){Pe.diagnosticTracing&&b("Resuming more parallel downloads");const e=L;L=void 0,e.promise_control.resolve()}}}function le(e,t){let o;return null==t&&Be(!1,`sourcePrefix must be provided for ${e.name}`),e.resolvedUrl?o=e.resolvedUrl:(o=""===t?"assembly"===e.behavior||"pdb"===e.behavior?e.name:"resource"===e.behavior&&e.culture&&""!==e.culture?`${e.culture}/${e.name}`:e.name:t+e.name,o=ce(Pe.locateFile(o),e.behavior)),o&&"string"==typeof o||Be(!1,"attemptUrl need to be path or url string"),o}function ce(e,t){return Pe.modulesUniqueQuery&&q[t]&&(e+=Pe.modulesUniqueQuery),e}let de=0;const ue=new Set;function fe(e){try{e.resolvedUrl||Be(!1,"Request's resolvedUrl must be set");const t=function(e){let t=e.resolvedUrl;if(Pe.loadBootResource){const o=ge(e);if(o instanceof Promise)return o;"string"==typeof o&&(t=o)}const o={};return e.cache?o.cache=e.cache:Pe.config.disableNoCacheFetch||(o.cache="no-cache"),e.useCredentials?o.credentials="include":!Pe.config.disableIntegrityCheck&&e.hash&&(o.integrity=e.hash),Pe.fetch_like(t,o)}(e),o={name:e.name,url:e.resolvedUrl,response:t};return ue.add(e.name),o.response.then((()=>{"assembly"==e.behavior&&Pe.loadedAssemblies.push(e.name),de++,Pe.onDownloadResourceProgress&&Pe.onDownloadResourceProgress(de,ue.size)})),o}catch(t){const o={ok:!1,url:e.resolvedUrl,status:500,statusText:"ERR29: "+t,arrayBuffer:()=>{throw t},json:()=>{throw t}};return{name:e.name,url:e.resolvedUrl,response:Promise.resolve(o)}}}const me={resource:"assembly",assembly:"assembly",pdb:"pdb",icu:"globalization",vfs:"configuration",manifest:"manifest",dotnetwasm:"dotnetwasm","js-module-dotnet":"dotnetjs","js-module-native":"dotnetjs","js-module-runtime":"dotnetjs","js-module-threads":"dotnetjs"};function ge(e){var t;if(Pe.loadBootResource){const o=null!==(t=e.hash)&&void 0!==t?t:"",n=e.resolvedUrl,r=me[e.behavior];if(r){const t=Pe.loadBootResource(r,e.name,n,o,e.behavior);return"string"==typeof t?I(t):t}}}function pe(e){e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null}function he(e){let t=e.lastIndexOf("/");return t>=0&&t++,e.substring(t)}async function we(e){e&&await Promise.all((null!=e?e:[]).map((e=>async function(e){try{const t=e.name;if(!e.moduleExports){const o=ce(Pe.locateFile(t),"js-module-library-initializer");Pe.diagnosticTracing&&b(`Attempting to import '${o}' for ${e}`),e.moduleExports=await import(/*! webpackIgnore: true */o)}Pe.libraryInitializers.push({scriptName:t,exports:e.moduleExports})}catch(t){E(`Failed to import library initializer '${e}': ${t}`)}}(e))))}async function be(e,t){if(!Pe.libraryInitializers)return;const o=[];for(let n=0;n<Pe.libraryInitializers.length;n++){const r=Pe.libraryInitializers[n];r.exports[e]&&o.push(ye(r.scriptName,e,(()=>r.exports[e](...t))))}await Promise.all(o)}async function ye(e,t,o){try{await o()}catch(o){throw E(`Failed to invoke '${t}' on library initializer '${e}': ${o}`),Xe(1,o),o}}function ve(e,t){if(e===t)return e;const o={...t};return void 0!==o.assets&&o.assets!==e.assets&&(o.assets=[...e.assets||[],...o.assets||[]]),void 0!==o.resources&&(o.resources=_e(e.resources||{assembly:[],jsModuleNative:[],jsModuleRuntime:[],wasmNative:[]},o.resources)),void 0!==o.environmentVariables&&(o.environmentVariables={...e.environmentVariables||{},...o.environmentVariables||{}}),void 0!==o.runtimeOptions&&o.runtimeOptions!==e.runtimeOptions&&(o.runtimeOptions=[...e.runtimeOptions||[],...o.runtimeOptions||[]]),Object.assign(e,o)}function Ee(e,t){if(e===t)return e;const o={...t};return o.config&&(e.config||(e.config={}),o.config=ve(e.config,o.config)),Object.assign(e,o)}function _e(e,t){if(e===t)return e;const o={...t};return void 0!==o.coreAssembly&&(o.coreAssembly=[...e.coreAssembly||[],...o.coreAssembly||[]]),void 0!==o.assembly&&(o.assembly=[...e.assembly||[],...o.assembly||[]]),void 0!==o.lazyAssembly&&(o.lazyAssembly=[...e.lazyAssembly||[],...o.lazyAssembly||[]]),void 0!==o.corePdb&&(o.corePdb=[...e.corePdb||[],...o.corePdb||[]]),void 0!==o.pdb&&(o.pdb=[...e.pdb||[],...o.pdb||[]]),void 0!==o.jsModuleWorker&&(o.jsModuleWorker=[...e.jsModuleWorker||[],...o.jsModuleWorker||[]]),void 0!==o.jsModuleNative&&(o.jsModuleNative=[...e.jsModuleNative||[],...o.jsModuleNative||[]]),void 0!==o.jsModuleDiagnostics&&(o.jsModuleDiagnostics=[...e.jsModuleDiagnostics||[],...o.jsModuleDiagnostics||[]]),void 0!==o.jsModuleRuntime&&(o.jsModuleRuntime=[...e.jsModuleRuntime||[],...o.jsModuleRuntime||[]]),void 0!==o.wasmSymbols&&(o.wasmSymbols=[...e.wasmSymbols||[],...o.wasmSymbols||[]]),void 0!==o.wasmNative&&(o.wasmNative=[...e.wasmNative||[],...o.wasmNative||[]]),void 0!==o.icu&&(o.icu=[...e.icu||[],...o.icu||[]]),void 0!==o.satelliteResources&&(o.satelliteResources=function(e,t){if(e===t)return e;for(const o in t)e[o]=[...e[o]||[],...t[o]||[]];return e}(e.satelliteResources||{},o.satelliteResources||{})),void 0!==o.modulesAfterConfigLoaded&&(o.modulesAfterConfigLoaded=[...e.modulesAfterConfigLoaded||[],...o.modulesAfterConfigLoaded||[]]),void 0!==o.modulesAfterRuntimeReady&&(o.modulesAfterRuntimeReady=[...e.modulesAfterRuntimeReady||[],...o.modulesAfterRuntimeReady||[]]),void 0!==o.extensions&&(o.extensions={...e.extensions||{},...o.extensions||{}}),void 0!==o.vfs&&(o.vfs=[...e.vfs||[],...o.vfs||[]]),Object.assign(e,o)}function xe(){const e=Pe.config;if(e.environmentVariables=e.environmentVariables||{},e.runtimeOptions=e.runtimeOptions||[],e.resources=e.resources||{assembly:[],jsModuleNative:[],jsModuleWorker:[],jsModuleRuntime:[],wasmNative:[],vfs:[],satelliteResources:{}},e.assets){Pe.diagnosticTracing&&b("config.assets is deprecated, use config.resources instead");for(const t of e.assets){const o={};switch(t.behavior){case"assembly":o.assembly=[t];break;case"pdb":o.pdb=[t];break;case"resource":o.satelliteResources={},o.satelliteResources[t.culture]=[t];break;case"icu":o.icu=[t];break;case"symbols":o.wasmSymbols=[t];break;case"vfs":o.vfs=[t];break;case"dotnetwasm":o.wasmNative=[t];break;case"js-module-threads":o.jsModuleWorker=[t];break;case"js-module-runtime":o.jsModuleRuntime=[t];break;case"js-module-native":o.jsModuleNative=[t];break;case"js-module-diagnostics":o.jsModuleDiagnostics=[t];break;case"js-module-dotnet":break;default:throw new Error(`Unexpected behavior ${t.behavior} of asset ${t.name}`)}_e(e.resources,o)}}e.debugLevel,e.applicationEnvironment||(e.applicationEnvironment="Production"),e.applicationCulture&&(e.environmentVariables.LANG=`${e.applicationCulture}.UTF-8`),Ue.diagnosticTracing=Pe.diagnosticTracing=!!e.diagnosticTracing,Ue.waitForDebugger=e.waitForDebugger,Pe.maxParallelDownloads=e.maxParallelDownloads||Pe.maxParallelDownloads,Pe.enableDownloadRetry=void 0!==e.enableDownloadRetry?e.enableDownloadRetry:Pe.enableDownloadRetry}let je=!1;async function Re(e){var t;if(je)return void await Pe.afterConfigLoaded.promise;let o;try{if(e.configSrc||Pe.config&&0!==Object.keys(Pe.config).length&&(Pe.config.assets||Pe.config.resources)||(e.configSrc="dotnet.boot.js"),o=e.configSrc,je=!0,o&&(Pe.diagnosticTracing&&b("mono_wasm_load_config"),await async function(e){const t=e.configSrc,o=Pe.locateFile(t);let n=null;void 0!==Pe.loadBootResource&&(n=Pe.loadBootResource("manifest",t,o,"","manifest"));let r,i=null;if(n)if("string"==typeof n)n.includes(".json")?(i=await s(I(n)),r=await Ae(i)):r=(await import(I(n))).config;else{const e=await n;"function"==typeof e.json?(i=e,r=await Ae(i)):r=e.config}else o.includes(".json")?(i=await s(ce(o,"manifest")),r=await Ae(i)):r=(await import(ce(o,"manifest"))).config;function s(e){return Pe.fetch_like(e,{method:"GET",credentials:"include",cache:"no-cache"})}Pe.config.applicationEnvironment&&(r.applicationEnvironment=Pe.config.applicationEnvironment),ve(Pe.config,r)}(e)),xe(),await we(null===(t=Pe.config.resources)||void 0===t?void 0:t.modulesAfterConfigLoaded),await be("onRuntimeConfigLoaded",[Pe.config]),e.onConfigLoaded)try{await e.onConfigLoaded(Pe.config,Le),xe()}catch(e){throw _("onConfigLoaded() failed",e),e}xe(),Pe.afterConfigLoaded.promise_control.resolve(Pe.config)}catch(t){const n=`Failed to load config file ${o} ${t} ${null==t?void 0:t.stack}`;throw Pe.config=e.config=Object.assign(Pe.config,{message:n,error:t,isError:!0}),Xe(1,new Error(n)),t}}function Te(){return!!globalThis.navigator&&(Pe.isChromium||Pe.isFirefox)}async function Ae(e){const t=Pe.config,o=await e.json();t.applicationEnvironment||o.applicationEnvironment||(o.applicationEnvironment=e.headers.get("Blazor-Environment")||e.headers.get("DotNet-Environment")||void 0),o.environmentVariables||(o.environmentVariables={});const n=e.headers.get("DOTNET-MODIFIABLE-ASSEMBLIES");n&&(o.environmentVariables.DOTNET_MODIFIABLE_ASSEMBLIES=n);const r=e.headers.get("ASPNETCORE-BROWSER-TOOLS");return r&&(o.environmentVariables.__ASPNETCORE_BROWSER_TOOLS=r),o}"function"!=typeof importScripts||globalThis.onmessage||(globalThis.dotnetSidecar=!0);const Se="object"==typeof process&&"object"==typeof process.versions&&"string"==typeof process.versions.node,De="function"==typeof importScripts,Oe=De&&"undefined"!=typeof dotnetSidecar,Ce=De&&!Oe,ke="object"==typeof window||De&&!Se,Ie=!ke&&!Se;let Ue={},Pe={},Me={},Le={},Ne={},$e=!1;const ze={},We={config:ze},Fe={mono:{},binding:{},internal:Ne,module:We,loaderHelpers:Pe,runtimeHelpers:Ue,diagnosticHelpers:Me,api:Le};function Be(e,t){if(e)return;const o="Assert failed: "+("function"==typeof t?t():t),n=new Error(o);_(o,n),Ue.nativeAbort(n)}function Ve(){return void 0!==Pe.exitCode}function qe(){return Ue.runtimeReady&&!Ve()}function He(){Ve()&&Be(!1,`.NET runtime already exited with ${Pe.exitCode} ${Pe.exitReason}. You can use runtime.runMain() which doesn't exit the runtime.`),Ue.runtimeReady||Be(!1,".NET runtime didn't start yet. Please call dotnet.create() first.")}function Je(){ke&&(globalThis.addEventListener("unhandledrejection",et),globalThis.addEventListener("error",tt))}let Ze,Qe;function Ge(e){Qe&&Qe(e),Xe(e,Pe.exitReason)}function Ke(e){Ze&&Ze(e||Pe.exitReason),Xe(1,e||Pe.exitReason)}function Xe(t,o){var n,r;const i=o&&"object"==typeof o;t=i&&"number"==typeof o.status?o.status:void 0===t?-1:t;const s=i&&"string"==typeof o.message?o.message:""+o;(o=i?o:Ue.ExitStatus?function(e,t){const o=new Ue.ExitStatus(e);return o.message=t,o.toString=()=>t,o}(t,s):new Error("Exit with code "+t+" "+s)).status=t,o.message||(o.message=s);const a=""+(o.stack||(new Error).stack);try{Object.defineProperty(o,"stack",{get:()=>a})}catch(e){}const l=!!o.silent;if(o.silent=!0,Ve())Pe.diagnosticTracing&&b("mono_exit called after exit");else{try{We.onAbort==Ke&&(We.onAbort=Ze),We.onExit==Ge&&(We.onExit=Qe),ke&&(globalThis.removeEventListener("unhandledrejection",et),globalThis.removeEventListener("error",tt)),Ue.runtimeReady?(Ue.jiterpreter_dump_stats&&Ue.jiterpreter_dump_stats(!1),0===t&&(null===(n=Pe.config)||void 0===n?void 0:n.interopCleanupOnExit)&&Ue.forceDisposeProxies(!0,!0),e&&0!==t&&(null===(r=Pe.config)||void 0===r||r.dumpThreadsOnNonZeroExit)):(Pe.diagnosticTracing&&b(`abort_startup, reason: ${o}`),function(e){Pe.allDownloadsQueued.promise_control.reject(e),Pe.allDownloadsFinished.promise_control.reject(e),Pe.afterConfigLoaded.promise_control.reject(e),Pe.wasmCompilePromise.promise_control.reject(e),Pe.runtimeModuleLoaded.promise_control.reject(e),Ue.dotnetReady&&(Ue.dotnetReady.promise_control.reject(e),Ue.afterInstantiateWasm.promise_control.reject(e),Ue.beforePreInit.promise_control.reject(e),Ue.afterPreInit.promise_control.reject(e),Ue.afterPreRun.promise_control.reject(e),Ue.beforeOnRuntimeInitialized.promise_control.reject(e),Ue.afterOnRuntimeInitialized.promise_control.reject(e),Ue.afterPostRun.promise_control.reject(e))}(o))}catch(e){E("mono_exit A failed",e)}try{l||(function(e,t){if(0!==e&&t){const e=Ue.ExitStatus&&t instanceof Ue.ExitStatus?b:_;"string"==typeof t?e(t):(void 0===t.stack&&(t.stack=(new Error).stack+""),t.message?e(Ue.stringify_as_error_with_stack?Ue.stringify_as_error_with_stack(t.message+"\n"+t.stack):t.message+"\n"+t.stack):e(JSON.stringify(t)))}!Ce&&Pe.config&&(Pe.config.logExitCode?Pe.config.forwardConsoleLogsToWS?R("WASM EXIT "+e):v("WASM EXIT "+e):Pe.config.forwardConsoleLogsToWS&&R())}(t,o),function(e){if(ke&&!Ce&&Pe.config&&Pe.config.appendElementOnExit&&document){const t=document.createElement("label");t.id="tests_done",0!==e&&(t.style.background="red"),t.innerHTML=""+e,document.body.appendChild(t)}}(t))}catch(e){E("mono_exit B failed",e)}Pe.exitCode=t,Pe.exitReason||(Pe.exitReason=o),!Ce&&Ue.runtimeReady&&We.runtimeKeepalivePop()}if(Pe.config&&Pe.config.asyncFlushOnExit&&0===t)throw(async()=>{try{await async function(){try{const e=await import(/*! webpackIgnore: true */"process"),t=e=>new Promise(((t,o)=>{e.on("error",o),e.end("","utf8",t)})),o=t(e.stderr),n=t(e.stdout);let r;const i=new Promise((e=>{r=setTimeout((()=>e("timeout")),1e3)}));await Promise.race([Promise.all([n,o]),i]),clearTimeout(r)}catch(e){_(`flushing std* streams failed: ${e}`)}}()}finally{Ye(t,o)}})(),o;Ye(t,o)}function Ye(e,t){if(Ue.runtimeReady&&Ue.nativeExit)try{Ue.nativeExit(e)}catch(e){!Ue.ExitStatus||e instanceof Ue.ExitStatus||E("set_exit_code_and_quit_now failed: "+e.toString())}if(0!==e||!ke)throw Se&&Ne.process?Ne.process.exit(e):Ue.quit&&Ue.quit(e,t),t}function et(e){ot(e,e.reason,"rejection")}function tt(e){ot(e,e.error,"error")}function ot(e,t,o){e.preventDefault();try{t||(t=new Error("Unhandled "+o)),void 0===t.stack&&(t.stack=(new Error).stack),t.stack=t.stack+"",t.silent||(_("Unhandled error:",t),Xe(1,t))}catch(e){}}!function(e){if($e)throw new Error("Loader module already loaded");$e=!0,Ue=e.runtimeHelpers,Pe=e.loaderHelpers,Me=e.diagnosticHelpers,Le=e.api,Ne=e.internal,Object.assign(Le,{INTERNAL:Ne,invokeLibraryInitializers:be}),Object.assign(e.module,{config:ve(ze,{environmentVariables:{}})});const r={mono_wasm_bindings_is_ready:!1,config:e.module.config,diagnosticTracing:!1,nativeAbort:e=>{throw e||new Error("abort")},nativeExit:e=>{throw new Error("exit:"+e)}},l={gitHash:"44525024595742ebe09023abe709df51de65009b",config:e.module.config,diagnosticTracing:!1,maxParallelDownloads:16,enableDownloadRetry:!0,_loaded_files:[],loadedFiles:[],loadedAssemblies:[],libraryInitializers:[],workerNextNumber:1,actual_downloaded_assets_count:0,actual_instantiated_assets_count:0,expected_downloaded_assets_count:0,expected_instantiated_assets_count:0,afterConfigLoaded:i(),allDownloadsQueued:i(),allDownloadsFinished:i(),wasmCompilePromise:i(),runtimeModuleLoaded:i(),loadingWorkers:i(),is_exited:Ve,is_runtime_running:qe,assert_runtime_running:He,mono_exit:Xe,createPromiseController:i,getPromiseController:s,assertIsControllablePromise:a,mono_download_assets:oe,resolve_single_asset_path:ee,setup_proxy_console:j,set_thread_prefix:w,installUnhandledErrorHandler:Je,retrieve_asset_download:ie,invokeLibraryInitializers:be,isDebuggingSupported:Te,exceptions:t,simd:n,relaxedSimd:o};Object.assign(Ue,r),Object.assign(Pe,l)}(Fe);let nt,rt,it,st=!1,at=!1;async function lt(e){if(!at){if(at=!0,ke&&Pe.config.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&j("main",globalThis.console,globalThis.location.origin),We||Be(!1,"Null moduleConfig"),Pe.config||Be(!1,"Null moduleConfig.config"),"function"==typeof e){const t=e(Fe.api);if(t.ready)throw new Error("Module.ready couldn't be redefined.");Object.assign(We,t),Ee(We,t)}else{if("object"!=typeof e)throw new Error("Can't use moduleFactory callback of createDotnetRuntime function.");Ee(We,e)}await async function(e){if(Se){const e=await import(/*! webpackIgnore: true */"process"),t=14;if(e.versions.node.split(".")[0]<t)throw new Error(`NodeJS at '${e.execPath}' has too low version '${e.versions.node}', please use at least ${t}. See also https://aka.ms/dotnet-wasm-features`)}const t=/*! webpackIgnore: true */import.meta.url,o=t.indexOf("?");var n;if(o>0&&(Pe.modulesUniqueQuery=t.substring(o)),Pe.scriptUrl=t.replace(/\\/g,"/").replace(/[?#].*/,""),Pe.scriptDirectory=(n=Pe.scriptUrl).slice(0,n.lastIndexOf("/"))+"/",Pe.locateFile=e=>"URL"in globalThis&&globalThis.URL!==C?new URL(e,Pe.scriptDirectory).toString():M(e)?e:Pe.scriptDirectory+e,Pe.fetch_like=k,Pe.out=console.log,Pe.err=console.error,Pe.onDownloadResourceProgress=e.onDownloadResourceProgress,ke&&globalThis.navigator){const e=globalThis.navigator,t=e.userAgentData&&e.userAgentData.brands;t&&t.length>0?Pe.isChromium=t.some((e=>"Google Chrome"===e.brand||"Microsoft Edge"===e.brand||"Chromium"===e.brand)):e.userAgent&&(Pe.isChromium=e.userAgent.includes("Chrome"),Pe.isFirefox=e.userAgent.includes("Firefox"))}Ne.require=Se?await import(/*! webpackIgnore: true */"module").then((e=>e.createRequire(/*! webpackIgnore: true */import.meta.url))):Promise.resolve((()=>{throw new Error("require not supported")})),void 0===globalThis.URL&&(globalThis.URL=C)}(We)}}async function ct(e){return await lt(e),Ze=We.onAbort,Qe=We.onExit,We.onAbort=Ke,We.onExit=Ge,We.ENVIRONMENT_IS_PTHREAD?async function(){(function(){const e=new MessageChannel,t=e.port1,o=e.port2;t.addEventListener("message",(e=>{var n,r;n=JSON.parse(e.data.config),r=JSON.parse(e.data.monoThreadInfo),st?Pe.diagnosticTracing&&b("mono config already received"):(ve(Pe.config,n),Ue.monoThreadInfo=r,xe(),Pe.diagnosticTracing&&b("mono config received"),st=!0,Pe.afterConfigLoaded.promise_control.resolve(Pe.config),ke&&n.forwardConsoleLogsToWS&&void 0!==globalThis.WebSocket&&Pe.setup_proxy_console("worker-idle",console,globalThis.location.origin)),t.close(),o.close()}),{once:!0}),t.start(),self.postMessage({[l]:{monoCmd:"preload",port:o}},[o])})(),await Pe.afterConfigLoaded.promise,function(){const e=Pe.config;e.assets||Be(!1,"config.assets must be defined");for(const t of e.assets)X(t),Q[t.behavior]&&z.push(t)}(),setTimeout((async()=>{try{await oe()}catch(e){Xe(1,e)}}),0);const e=dt(),t=await Promise.all(e);return await ut(t),We}():async function(){var e;await Re(We),re();const t=dt();(async function(){try{const e=ee("dotnetwasm");await se(e),e&&e.pendingDownloadInternal&&e.pendingDownloadInternal.response||Be(!1,"Can't load dotnet.native.wasm");const t=await e.pendingDownloadInternal.response,o=t.headers&&t.headers.get?t.headers.get("Content-Type"):void 0;let n;if("function"==typeof WebAssembly.compileStreaming&&"application/wasm"===o)n=await WebAssembly.compileStreaming(t);else{ke&&"application/wasm"!==o&&E('WebAssembly resource does not have the expected content type "application/wasm", so falling back to slower ArrayBuffer instantiation.');const e=await t.arrayBuffer();Pe.diagnosticTracing&&b("instantiate_wasm_module buffered"),n=Ie?await Promise.resolve(new WebAssembly.Module(e)):await WebAssembly.compile(e)}e.pendingDownloadInternal=null,e.pendingDownload=null,e.buffer=null,e.moduleExports=null,Pe.wasmCompilePromise.promise_control.resolve(n)}catch(e){Pe.wasmCompilePromise.promise_control.reject(e)}})(),setTimeout((async()=>{try{D(),await oe()}catch(e){Xe(1,e)}}),0);const o=await Promise.all(t);return await ut(o),await Ue.dotnetReady.promise,await we(null===(e=Pe.config.resources)||void 0===e?void 0:e.modulesAfterRuntimeReady),await be("onRuntimeReady",[Fe.api]),Le}()}function dt(){const e=ee("js-module-runtime"),t=ee("js-module-native");if(nt&&rt)return[nt,rt,it];"object"==typeof e.moduleExports?nt=e.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${e.resolvedUrl}' for ${e.name}`),nt=import(/*! webpackIgnore: true */e.resolvedUrl)),"object"==typeof t.moduleExports?rt=t.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${t.resolvedUrl}' for ${t.name}`),rt=import(/*! webpackIgnore: true */t.resolvedUrl));const o=Y("js-module-diagnostics");return o&&("object"==typeof o.moduleExports?it=o.moduleExports:(Pe.diagnosticTracing&&b(`Attempting to import '${o.resolvedUrl}' for ${o.name}`),it=import(/*! webpackIgnore: true */o.resolvedUrl))),[nt,rt,it]}async function ut(e){const{initializeExports:t,initializeReplacements:o,configureRuntimeStartup:n,configureEmscriptenStartup:r,configureWorkerStartup:i,setRuntimeGlobals:s,passEmscriptenInternals:a}=e[0],{default:l}=e[1],c=e[2];s(Fe),t(Fe),c&&c.setRuntimeGlobals(Fe),await n(We),Pe.runtimeModuleLoaded.promise_control.resolve(),l((e=>(Object.assign(We,{ready:e.ready,__dotnet_runtime:{initializeReplacements:o,configureEmscriptenStartup:r,configureWorkerStartup:i,passEmscriptenInternals:a}}),We))).catch((e=>{if(e.message&&e.message.toLowerCase().includes("out of memory"))throw new Error(".NET runtime has failed to start, because too much memory was requested. Please decrease the memory by adjusting EmccMaximumHeapSize. See also https://aka.ms/dotnet-wasm-features");throw e}))}const ft=new class{withModuleConfig(e){try{return Ee(We,e),this}catch(e){throw Xe(1,e),e}}withOnConfigLoaded(e){try{return Ee(We,{onConfigLoaded:e}),this}catch(e){throw Xe(1,e),e}}withConsoleForwarding(){try{return ve(ze,{forwardConsoleLogsToWS:!0}),this}catch(e){throw Xe(1,e),e}}withExitOnUnhandledError(){try{return ve(ze,{exitOnUnhandledError:!0}),Je(),this}catch(e){throw Xe(1,e),e}}withAsyncFlushOnExit(){try{return ve(ze,{asyncFlushOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withExitCodeLogging(){try{return ve(ze,{logExitCode:!0}),this}catch(e){throw Xe(1,e),e}}withElementOnExit(){try{return ve(ze,{appendElementOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withInteropCleanupOnExit(){try{return ve(ze,{interopCleanupOnExit:!0}),this}catch(e){throw Xe(1,e),e}}withDumpThreadsOnNonZeroExit(){try{return ve(ze,{dumpThreadsOnNonZeroExit:!0}),this}catch(e){throw Xe(1,e),e}}withWaitingForDebugger(e){try{return ve(ze,{waitForDebugger:e}),this}catch(e){throw Xe(1,e),e}}withInterpreterPgo(e,t){try{return ve(ze,{interpreterPgo:e,interpreterPgoSaveDelay:t}),ze.runtimeOptions?ze.runtimeOptions.push("--interp-pgo-recording"):ze.runtimeOptions=["--interp-pgo-recording"],this}catch(e){throw Xe(1,e),e}}withConfig(e){try{return ve(ze,e),this}catch(e){throw Xe(1,e),e}}withConfigSrc(e){try{return e&&"string"==typeof e||Be(!1,"must be file path or URL"),Ee(We,{configSrc:e}),this}catch(e){throw Xe(1,e),e}}withVirtualWorkingDirectory(e){try{return e&&"string"==typeof e||Be(!1,"must be directory path"),ve(ze,{virtualWorkingDirectory:e}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariable(e,t){try{const o={};return o[e]=t,ve(ze,{environmentVariables:o}),this}catch(e){throw Xe(1,e),e}}withEnvironmentVariables(e){try{return e&&"object"==typeof e||Be(!1,"must be dictionary object"),ve(ze,{environmentVariables:e}),this}catch(e){throw Xe(1,e),e}}withDiagnosticTracing(e){try{return"boolean"!=typeof e&&Be(!1,"must be boolean"),ve(ze,{diagnosticTracing:e}),this}catch(e){throw Xe(1,e),e}}withDebugging(e){try{return null!=e&&"number"==typeof e||Be(!1,"must be number"),ve(ze,{debugLevel:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArguments(...e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ve(ze,{applicationArguments:e}),this}catch(e){throw Xe(1,e),e}}withRuntimeOptions(e){try{return e&&Array.isArray(e)||Be(!1,"must be array of strings"),ze.runtimeOptions?ze.runtimeOptions.push(...e):ze.runtimeOptions=e,this}catch(e){throw Xe(1,e),e}}withMainAssembly(e){try{return ve(ze,{mainAssemblyName:e}),this}catch(e){throw Xe(1,e),e}}withApplicationArgumentsFromQuery(){try{if(!globalThis.window)throw new Error("Missing window to the query parameters from");if(void 0===globalThis.URLSearchParams)throw new Error("URLSearchParams is supported");const e=new URLSearchParams(globalThis.window.location.search).getAll("arg");return this.withApplicationArguments(...e)}catch(e){throw Xe(1,e),e}}withApplicationEnvironment(e){try{return ve(ze,{applicationEnvironment:e}),this}catch(e){throw Xe(1,e),e}}withApplicationCulture(e){try{return ve(ze,{applicationCulture:e}),this}catch(e){throw Xe(1,e),e}}withResourceLoader(e){try{return Pe.loadBootResource=e,this}catch(e){throw Xe(1,e),e}}async download(){try{await async function(){lt(We),await Re(We),re(),D(),oe(),await Pe.allDownloadsFinished.promise}()}catch(e){throw Xe(1,e),e}}async create(){try{return this.instance||(this.instance=await async function(){return await ct(We),Fe.api}()),this.instance}catch(e){throw Xe(1,e),e}}async run(){try{return We.config||Be(!1,"Null moduleConfig.config"),this.instance||await this.create(),this.instance.runMainAndExit()}catch(e){throw Xe(1,e),e}}},mt=Xe,gt=ct;Ie||"function"==typeof globalThis.URL||Be(!1,"This browser/engine doesn't support URL API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),"function"!=typeof globalThis.BigInt64Array&&Be(!1,"This browser/engine doesn't support BigInt64Array API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"),ft.withConfig(/*json-start*/{
  "mainAssemblyName": "Demo",
  "resources": {
    "hash": "sha256-JwPvujkqiQNsqrXSuogiU9cncBRuJZcz70bhLIF1fwE=",
    "jsModuleNative": [
      {
        "name": "dotnet.native.7dr2sqrzoc.js"
      }
    ],
    "jsModuleRuntime": [
      {
        "name": "dotnet.runtime.2tx45g8lli.js"
      }
    ],
    "wasmNative": [
      {
        "name": "dotnet.native.anwtu157lx.wasm",
        "integrity": "sha256-eiO3Ig4TZIeoctl9PGCxlVYTdLC6V9MnGtX11KhYoV4=",
        "cache": "force-cache"
      }
    ],
    "icu": [
      {
        "virtualPath": "icudt_CJK.dat",
        "name": "icudt_CJK.tjcz0u77k5.dat",
        "integrity": "sha256-SZLtQnRc0JkwqHab0VUVP7T3uBPSeYzxzDnpxPpUnHk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "icudt_EFIGS.dat",
        "name": "icudt_EFIGS.tptq2av103.dat",
        "integrity": "sha256-8fItetYY8kQ0ww6oxwTLiT3oXlBwHKumbeP2pRF4yTc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "icudt_no_CJK.dat",
        "name": "icudt_no_CJK.lfu7j35m59.dat",
        "integrity": "sha256-L7sV7NEYP37/Qr2FPCePo5cJqRgTXRwGHuwF5Q+0Nfs=",
        "cache": "force-cache"
      }
    ],
    "coreAssembly": [
      {
        "virtualPath": "System.Runtime.InteropServices.JavaScript.wasm",
        "name": "System.Runtime.InteropServices.JavaScript.7p3x45v050.wasm",
        "integrity": "sha256-BcdkkYhr3AGnhko6oyHeO5F9QD7hsU/Fg3l6at/Yjao=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.CoreLib.wasm",
        "name": "System.Private.CoreLib.44tsu6697z.wasm",
        "integrity": "sha256-nCddKY1D+0XHCu5VsPSy6Dpwg9smsYvYxAwAja2Cz5Q=",
        "cache": "force-cache"
      }
    ],
    "assembly": [
      {
        "virtualPath": "Azure.AI.OpenAI.wasm",
        "name": "Azure.AI.OpenAI.izj689wdj3.wasm",
        "integrity": "sha256-rrZrs5/4lvJNweABn/NS9YtgMjWXXiW2MBPS5dGwQV4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Azure.Core.wasm",
        "name": "Azure.Core.ltf0pljmuu.wasm",
        "integrity": "sha256-59JxV5wTM2SQr+SUghmDjF366cxcEFzb01pIZFbSQvE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Blazored.LocalStorage.wasm",
        "name": "Blazored.LocalStorage.12n6dz54qr.wasm",
        "integrity": "sha256-OaMAAd5n7ORfyur5e3QIyEVKJ76MKIvwbg7/icnnYcU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "BouncyCastle.Crypto.wasm",
        "name": "BouncyCastle.Crypto.1rz6faqotu.wasm",
        "integrity": "sha256-g47N1IoyzTS8v1Lp+LjGCg4SUnnnhVra8P4e3+koXgU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "BouncyCastle.Cryptography.wasm",
        "name": "BouncyCastle.Cryptography.6qnbenkd0k.wasm",
        "integrity": "sha256-IqnVnhA0e0WF9/Ufx8/JIbwvVOGRIsbVT0pJuf86K70=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "DimonSmart.AiUtils.wasm",
        "name": "DimonSmart.AiUtils.qgk9rrbjel.wasm",
        "integrity": "sha256-2oYg0ygmpzpebZbQcyE/6NNe88Ett1w/Bw5lTUQ6NkA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "DimonSmart.HashX.wasm",
        "name": "DimonSmart.HashX.pc1h9fshzl.wasm",
        "integrity": "sha256-GWbxNAvpJtxUlRfaqRw2Lgksx+9CYEfaRD5icDhoi8U=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "DimonSmart.MazeGenerator.wasm",
        "name": "DimonSmart.MazeGenerator.s2r6yx7vnk.wasm",
        "integrity": "sha256-bHv6o0rhGbvzZuAq6JHNDFKkbmZ2cr9T3uaCh0jwkt4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "DimonSmart.PdfCropper.wasm",
        "name": "DimonSmart.PdfCropper.u6womgx70x.wasm",
        "integrity": "sha256-9p4YEgMz1c56DRDfkily5ceyWC46QRc91vA4FNcOwQA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "DimonSmart.StringDiff.wasm",
        "name": "DimonSmart.StringDiff.g2czv7cqeb.wasm",
        "integrity": "sha256-2GIfD8XW3skZBe8HByCkf1+YysabyQiy8R3Lq02xfso=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Blazor.Canvas.wasm",
        "name": "Blazor.Canvas.f4j2trvhby.wasm",
        "integrity": "sha256-T4ZgGnBBqgFn7l9BvL+qKQ2iaa4XQcYzJtX0D9NLX2U=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Excubo.Generators.Blazor.Attributes.wasm",
        "name": "Excubo.Generators.Blazor.Attributes.zpv3sgtlrf.wasm",
        "integrity": "sha256-qVC+LIPC1NiSLnwtgPgDy8x2ViPcFHEh0j+pB1GfDyk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.barcodes.wasm",
        "name": "itext.barcodes.krmk9ilev7.wasm",
        "integrity": "sha256-wE6MKtg6rP3ngHNzaVBRAmB5D/93y8XH3+SVbGAEyoo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.bouncy-castle-connector.wasm",
        "name": "itext.bouncy-castle-connector.ekfkk5wm8j.wasm",
        "integrity": "sha256-ztMzZIB8BCMBYgFPT8ihq6nGt/eo0EL1PMq6LL6Dolw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.forms.wasm",
        "name": "itext.forms.6xv36uu3cd.wasm",
        "integrity": "sha256-WmKKQ1nr9t09a/Rg382dJ8itfsukHhwRYLYmg7ecD/k=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.io.wasm",
        "name": "itext.io.i15se22plk.wasm",
        "integrity": "sha256-eoG4Q44Z1gaW8FbLd3QeWr94445rKD0jvAjRVBvC6iA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.kernel.wasm",
        "name": "itext.kernel.7pkxr3b04j.wasm",
        "integrity": "sha256-9YG+r4UzvPKBjsk7qpZ6sEN+RsfEOWfTFdrBSek33Z0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.layout.wasm",
        "name": "itext.layout.e5mbzapctp.wasm",
        "integrity": "sha256-HsCnY5shqdvk22B6fOXeB/UVfCHu4+dH9ACSlkpnc54=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.pdfa.wasm",
        "name": "itext.pdfa.hhe84zblae.wasm",
        "integrity": "sha256-qyTMDT/LrzXOvyVQBvryLN6R3ChsuFXjP6EDXbhHwak=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.pdfua.wasm",
        "name": "itext.pdfua.kjwgpox1rf.wasm",
        "integrity": "sha256-wHHytXFI22IIj2QzQ3cIheCusaJsAWdJ94t3yF2qx8s=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.sign.wasm",
        "name": "itext.sign.ap35o7megt.wasm",
        "integrity": "sha256-Aupjt6XAE0yeZpdakxKlnvyfvX3eH9afwoH8pxZ4AOs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.styledxmlparser.wasm",
        "name": "itext.styledxmlparser.zv5d34nx7w.wasm",
        "integrity": "sha256-D4Joqxuxaa4gNsh5CIHd1O948QyOidpemXFdR56LXgI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.svg.wasm",
        "name": "itext.svg.paka06fpxb.wasm",
        "integrity": "sha256-QY3KcG9kzqY4CG3nrphBWehW8/P15oc17XgGRpvePDY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.bouncy-castle-adapter.wasm",
        "name": "itext.bouncy-castle-adapter.2y676al523.wasm",
        "integrity": "sha256-FiOFUzty9djcwplPjDmHfJthQP1bZ9aXt5ghe9Ae7Eo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "itext.commons.wasm",
        "name": "itext.commons.r7wq6zq46n.wasm",
        "integrity": "sha256-aQjckEyo48P6IOCoTZl6uHRtEZHvxSUKREczpEXw84M=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "KristofferStrube.Blazor.DOM.wasm",
        "name": "KristofferStrube.Blazor.DOM.3ql762v36c.wasm",
        "integrity": "sha256-RdiHmZkJK6ChBUODYuurrA/AJOxcHi0tPLsEHWdfkfA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "KristofferStrube.Blazor.MediaCaptureStreams.wasm",
        "name": "KristofferStrube.Blazor.MediaCaptureStreams.cm47elfatf.wasm",
        "integrity": "sha256-JMWE96HAfcpe5UVbWy+752g8PIHzAy9KCBFlfy0W5qo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "KristofferStrube.Blazor.WebIDL.wasm",
        "name": "KristofferStrube.Blazor.WebIDL.yf6qm9vjz4.wasm",
        "integrity": "sha256-4hLFLRJ5CXuhxl9k7wrHtW/r8lxVip1NwMXQ5pKHIvA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Markdig.wasm",
        "name": "Markdig.y4i0b9whld.wasm",
        "integrity": "sha256-LrX4yrb2vyi8O8AjNTkqYrOAMLTaPBVsT/Fjji1HxIY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.wasm",
        "name": "Microsoft.AspNetCore.Components.oc73lnso5j.wasm",
        "integrity": "sha256-CAfCwV3f3dKgqXHL3r+UMqAZOuT1pGLOVPQY/Zk1bcA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Forms.wasm",
        "name": "Microsoft.AspNetCore.Components.Forms.bu9hhccp18.wasm",
        "integrity": "sha256-E62ZiRQeF6EqWRwORFWEdCqSB910+jhdffaWuLiu+LA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.Web.wasm",
        "name": "Microsoft.AspNetCore.Components.Web.fk9eousxk8.wasm",
        "integrity": "sha256-73fAJ49+a8E5EV6/cicd3S2p7htBCjA95IlSyGSkLQw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.AspNetCore.Components.WebAssembly.wasm",
        "name": "Microsoft.AspNetCore.Components.WebAssembly.ammtg6e8yd.wasm",
        "integrity": "sha256-PiCGZBCj/FDlUS9SG7+vOEznbRqiI2/ML4+QngZXukE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Bcl.HashCode.wasm",
        "name": "Microsoft.Bcl.HashCode.gfbo4c6st8.wasm",
        "integrity": "sha256-3OMNy/2VRU1BJsB1XSppo6yKKvhDQv3fcHDVi5jGv3k=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.DotNet.PlatformAbstractions.wasm",
        "name": "Microsoft.DotNet.PlatformAbstractions.l5f1k5rpem.wasm",
        "integrity": "sha256-+bp3smw6HF4P5NLzNAbp7j/Fzp163jJn8MhPIgaJpu0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.AI.wasm",
        "name": "Microsoft.Extensions.AI.qabla4yus9.wasm",
        "integrity": "sha256-jll6F6sdb/6fLqmuzXITCoiBUFIXqzEbDYAI5ShHCJY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.AI.Abstractions.wasm",
        "name": "Microsoft.Extensions.AI.Abstractions.x1vo4zc838.wasm",
        "integrity": "sha256-NWM2DTwAWb5mOECjfdDyIN7+L1/J7ss2NWYqQsXWX9E=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.AI.OpenAI.wasm",
        "name": "Microsoft.Extensions.AI.OpenAI.9zdpb7k0ls.wasm",
        "integrity": "sha256-+0VHiGoTwpGTyhnfmaTzX8xUZ42qUVCRQG2nAxnpfNo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.wasm",
        "name": "Microsoft.Extensions.Configuration.a4wnxspypz.wasm",
        "integrity": "sha256-WsPaWY3kqOkK4ebqxHG4bMXA9lwwVloh2Q1ZL8cPXlQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Abstractions.wasm",
        "name": "Microsoft.Extensions.Configuration.Abstractions.6f7dm07g1b.wasm",
        "integrity": "sha256-0qzXz6a+BPsxkNviyJa9zNUx3E45yFAsQirvu1glEwk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.FileExtensions.wasm",
        "name": "Microsoft.Extensions.Configuration.FileExtensions.ytp1qhtqc7.wasm",
        "integrity": "sha256-Bsef8YasIikyx/nMfAOyEx11QURjllo93jaYg95g4VU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.Json.wasm",
        "name": "Microsoft.Extensions.Configuration.Json.zqnrdmla8x.wasm",
        "integrity": "sha256-BTo+KvtnI4Ct8nBwTlxFuQ+e+un4hrbD7JTUlRjV1Oc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Configuration.UserSecrets.wasm",
        "name": "Microsoft.Extensions.Configuration.UserSecrets.w0uk56lb4c.wasm",
        "integrity": "sha256-AB7kH15XbE/LTOEg5hzTj9rJDm+tBMi4hGquPJvY3Q8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.wasm",
        "name": "Microsoft.Extensions.DependencyInjection.dteetxxxnv.wasm",
        "integrity": "sha256-gfOKJ6Ivjsd7AyQ0r+RGf0l+S+gkm4T3oCbHjZpYOWA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyInjection.Abstractions.wasm",
        "name": "Microsoft.Extensions.DependencyInjection.Abstractions.kqajkndch7.wasm",
        "integrity": "sha256-5CKB/uIz50SejtVEh/D42MfwqBGfLJgh4R6Tfjzng1Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.DependencyModel.wasm",
        "name": "Microsoft.Extensions.DependencyModel.zdp1aopd0u.wasm",
        "integrity": "sha256-x7DaGuO1ymR33gWsMyg1G10IlzKBMmnPMFaA7S3T+l0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.wasm",
        "name": "Microsoft.Extensions.Diagnostics.6905uxqau5.wasm",
        "integrity": "sha256-irLbKe8IiqhEhzTIPFMbZ5ljaTPMqYyoGFHgPNNvfEY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Diagnostics.Abstractions.wasm",
        "name": "Microsoft.Extensions.Diagnostics.Abstractions.yymkixmuzz.wasm",
        "integrity": "sha256-Sgex+pDO6d1gAzxVQ50g2WGmurdNj2DATQ3Vsg5+wGM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.FileProviders.Abstractions.wasm",
        "name": "Microsoft.Extensions.FileProviders.Abstractions.gg4bdgl79r.wasm",
        "integrity": "sha256-sJd7yecIbRmSwW/wuUbTlPmhXZ3Z6o4d8gsAZSGz+SQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.FileProviders.Physical.wasm",
        "name": "Microsoft.Extensions.FileProviders.Physical.rvz1ea662i.wasm",
        "integrity": "sha256-GX4TcuhD7PX1uNXS8b6iTv8l1zSBo1qi6EEe9g2263k=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.FileSystemGlobbing.wasm",
        "name": "Microsoft.Extensions.FileSystemGlobbing.8kcyumaw2z.wasm",
        "integrity": "sha256-GArgJeT9SIUI6nOcPUYAlLP8iKmenuZGyU60NBnaokM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Http.wasm",
        "name": "Microsoft.Extensions.Http.cs91g3m5dd.wasm",
        "integrity": "sha256-NHyxLpNrre6bHalnqfrV2K/sf5+lJ2Byv4CQnd7+97g=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.wasm",
        "name": "Microsoft.Extensions.Logging.9zrt1n84db.wasm",
        "integrity": "sha256-5Q9eycGRhSgebgszhRz1VJqIFohJIJo/Y/2ijBn5bB4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Logging.Abstractions.wasm",
        "name": "Microsoft.Extensions.Logging.Abstractions.hpswt0jlf7.wasm",
        "integrity": "sha256-q/lshY6yf/yLpClIFHSXlD1oAAEdFoOzf4IM1W6L/lM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Options.wasm",
        "name": "Microsoft.Extensions.Options.nk0l8h857s.wasm",
        "integrity": "sha256-J6JHcCY0ZXEusRvk8E63oTuYcl3HwgcLe3wZ17rpVlE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.Primitives.wasm",
        "name": "Microsoft.Extensions.Primitives.q73fe78haa.wasm",
        "integrity": "sha256-+jeHTg7D7ZOak/wYLi7lsVo2cPrmWsIBhYFg6m1Qndo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.Extensions.VectorData.Abstractions.wasm",
        "name": "Microsoft.Extensions.VectorData.Abstractions.0j1z51ppvb.wasm",
        "integrity": "sha256-fIvbDpchaWQ80TFS2fVxznbi/SKaRUziIlcAItaOkkM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.JSInterop.wasm",
        "name": "Microsoft.JSInterop.9i94lm8vnv.wasm",
        "integrity": "sha256-IZacpQNYMtARsvSnLZEIHVph6MC3OXn8A/pv6O3+xhk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.JSInterop.WebAssembly.wasm",
        "name": "Microsoft.JSInterop.WebAssembly.58jjh3bwur.wasm",
        "integrity": "sha256-FTVJx7gilBV/VWu+ibz7K8ueo7JUXPGj94aH0rf2FXQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.SemanticKernel.wasm",
        "name": "Microsoft.SemanticKernel.8texiy8ny2.wasm",
        "integrity": "sha256-PCPZXqP2PX5xq9hdrfHxcV9VRAB9U8sHSmtvB2ARqI0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.SemanticKernel.Abstractions.wasm",
        "name": "Microsoft.SemanticKernel.Abstractions.qt3xspq63o.wasm",
        "integrity": "sha256-Yq7wvrA0C3XJD9MxLJtd+1o7z4AzVaJB7OTjD82tOgQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.SemanticKernel.Agents.Abstractions.wasm",
        "name": "Microsoft.SemanticKernel.Agents.Abstractions.1j89cpoyzr.wasm",
        "integrity": "sha256-bSNRQkTSpS4ltnNd1W579j+8Q6ojpGkwdaeJznrsYQk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.SemanticKernel.Agents.Core.wasm",
        "name": "Microsoft.SemanticKernel.Agents.Core.mq8tnods90.wasm",
        "integrity": "sha256-fnj9mtOkm2Vbl/K5vsxzrrlVbtB8IrPh9C0LbA7iLIo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.SemanticKernel.Connectors.AzureOpenAI.wasm",
        "name": "Microsoft.SemanticKernel.Connectors.AzureOpenAI.bmk8eoixkj.wasm",
        "integrity": "sha256-Y6hh1UZWB9VIrg+1TCS5p5A6ihDi2FMmD+Ntgt/O2nI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.SemanticKernel.Connectors.Ollama.wasm",
        "name": "Microsoft.SemanticKernel.Connectors.Ollama.dhj9ea7hfr.wasm",
        "integrity": "sha256-H0/h8z/5ctKga/swQTdUfJ+dDAzkoqfPWniO9wmVghk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.SemanticKernel.Connectors.OpenAI.wasm",
        "name": "Microsoft.SemanticKernel.Connectors.OpenAI.1qp9bace1z.wasm",
        "integrity": "sha256-h4JLLCOWxdubbW1vnWdXupEQU2z09oMRBS900O4kvx8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.SemanticKernel.Core.wasm",
        "name": "Microsoft.SemanticKernel.Core.hmfrwtqfsf.wasm",
        "integrity": "sha256-2ZvhSxFWvDQxHiGqKyj+ybe889kmwcxWVwkP76CrdVQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "QrCodeGenerator.wasm",
        "name": "QrCodeGenerator.q8u0st2ezq.wasm",
        "integrity": "sha256-GmkDNeQd6vooNTrJh+av3yjvSjHWN17lPAbe7xJ8Oy4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Newtonsoft.Json.wasm",
        "name": "Newtonsoft.Json.se2bgyj0tw.wasm",
        "integrity": "sha256-DBfpbduzEP2lTeFZddrFGuNDOcOwgBPZwFsTYJEFsSA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "OllamaSharp.wasm",
        "name": "OllamaSharp.4sy5g016yb.wasm",
        "integrity": "sha256-rCAPOYeiC5yIieNALHKCxoFYDH64YFfb5F1eMoHtSg0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "OpenAI.wasm",
        "name": "OpenAI.u00t99a0nd.wasm",
        "integrity": "sha256-ok0zAtiKr1zhbLmlXt9c9Qo/b8gWTBUVfdoAp90iUiQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "PDFtoImage.wasm",
        "name": "PDFtoImage.n333xnyvtk.wasm",
        "integrity": "sha256-c4/7jPnqGdgtNqRhppGgLNnJ9iKEVMlArM9yQYF0Uwk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "SkiaSharp.wasm",
        "name": "SkiaSharp.vena8b6h9c.wasm",
        "integrity": "sha256-B0nvvBhUgqIb3oMr9pvGvhDYlbgpB3Vo11kelJOPQpY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "SkiaSharp.Views.Blazor.wasm",
        "name": "SkiaSharp.Views.Blazor.74nar1fiep.wasm",
        "integrity": "sha256-eV3F6NH5/nfLcQMQAloe6sGD4/VtH983nxzPYykRU30=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ClientModel.wasm",
        "name": "System.ClientModel.fp5zr9rja2.wasm",
        "integrity": "sha256-ozYUhks2Qz1HQ23n+Hk9fmI78KAAJnUzgitfOpEFgBc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.Async.wasm",
        "name": "System.Linq.Async.k9im717kse.wasm",
        "integrity": "sha256-CH9HlSymBG/b7dH7jcPxKNmsHaT3OBrjgrUGQqTWkBo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Memory.Data.wasm",
        "name": "System.Memory.Data.lvei4swmm5.wasm",
        "integrity": "sha256-L/CcPX3sCeOINTQhTOakytId+rTIiFySXxKA2/7ltSE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Pkcs.wasm",
        "name": "System.Security.Cryptography.Pkcs.9p871vd33d.wasm",
        "integrity": "sha256-zTQuKZzojqlM6Z8VnuF6RHU+askNwcPB5pLROzrWMCs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Xml.wasm",
        "name": "System.Security.Cryptography.Xml.hfbvywv1mj.wasm",
        "integrity": "sha256-Y6JKyQ1c2Z3liTt5KRyaJNqO7vwhBKmAaPMqSxpgdyo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "zxing.wasm",
        "name": "zxing.itfs5po1xb.wasm",
        "integrity": "sha256-6te88NRihcr5bnNB91pxN8zVJQeyGgCMM9gXwe1GHcM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Microsoft.CSharp.wasm",
        "name": "Microsoft.CSharp.1kpwb0en50.wasm",
        "integrity": "sha256-nytfqyGApiaaLwxiDSKypjq2NyctR04bg+3HtAGjZWY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.AppContext.wasm",
        "name": "System.AppContext.h3d26fs3dt.wasm",
        "integrity": "sha256-zZB85oFLG3VzPhN1WS7EcfOrTQazFkCv/gjQQKye43g=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Concurrent.wasm",
        "name": "System.Collections.Concurrent.vksgq0nj2m.wasm",
        "integrity": "sha256-r37J0uBrTUJFj7WOLi7fHs/ylgO9PGP6Nzm5mpIZbpU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Immutable.wasm",
        "name": "System.Collections.Immutable.97zb8rux3n.wasm",
        "integrity": "sha256-9foNr0AxEuV2nQxr/5qrqWd4aK6QYlayU6+pHcPNIjc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.NonGeneric.wasm",
        "name": "System.Collections.NonGeneric.gv3te6mnyr.wasm",
        "integrity": "sha256-JIHDcPXi1f33zHYV4YY68v6Z2P5jvu4PoWNsVp1uY8c=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.Specialized.wasm",
        "name": "System.Collections.Specialized.fr5079ztmr.wasm",
        "integrity": "sha256-K7hBfb9ieqAp9odVjiiJ4FY2Daas5qttCbf4ED8aX+w=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Collections.wasm",
        "name": "System.Collections.mh6k4wzkcs.wasm",
        "integrity": "sha256-2pERww2p9PpzU8VzKe+0WkffCeDsHz8tS4O5r15NW9E=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.Primitives.wasm",
        "name": "System.ComponentModel.Primitives.lpzjf03t5l.wasm",
        "integrity": "sha256-iRqy64yfZstcpzIsXvmf72RSmgUk0yq47SqluZ0Xypc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.TypeConverter.wasm",
        "name": "System.ComponentModel.TypeConverter.e2ctcwgj6o.wasm",
        "integrity": "sha256-Vq6SsIm+6RkXb0RBJEEMH19XwE1cOINlGY3Wk2RtkbI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ComponentModel.wasm",
        "name": "System.ComponentModel.zvpmoupymb.wasm",
        "integrity": "sha256-j4xVR2d1tpwZrUoag5ugeqAMDDfGXlE9q1rpeRzeLQY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Console.wasm",
        "name": "System.Console.kkeqv2vuie.wasm",
        "integrity": "sha256-RAObRJYeN7Z0xAvyCx9GCW+dslJm7nVKQeR5F6tmOmo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Data.Common.wasm",
        "name": "System.Data.Common.rnxwder2ya.wasm",
        "integrity": "sha256-mTh50zBgSpyv9ESgksU+TqPSjsAhDxKajtigZhcSw+w=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.Debug.wasm",
        "name": "System.Diagnostics.Debug.iyuivyptdk.wasm",
        "integrity": "sha256-xntL+WrNyDWdkoFvCSG1E1oS6iRPF7WPjk67qFtvmgM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.DiagnosticSource.wasm",
        "name": "System.Diagnostics.DiagnosticSource.rfn295a27f.wasm",
        "integrity": "sha256-2R+MYqFXDzaS2mC49SB0iPBGCUb3bwbv88ApRueJUgQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.Process.wasm",
        "name": "System.Diagnostics.Process.9kaiop3k4h.wasm",
        "integrity": "sha256-B67/9oCb4jbrKuiB0PZAQtzaj3wyUfOEm0/1y024kDA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.TraceSource.wasm",
        "name": "System.Diagnostics.TraceSource.erudlnv7dv.wasm",
        "integrity": "sha256-BoLnY2hATAvTS1w5Zb9CYfzT+oiPnQNO5293VGd9c0o=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Diagnostics.Tracing.wasm",
        "name": "System.Diagnostics.Tracing.025cq505e2.wasm",
        "integrity": "sha256-aI3qSeK3BCqeC0xjLPp+Wrb/pC1HAZj4IW5gU7VdyQo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Drawing.Primitives.wasm",
        "name": "System.Drawing.Primitives.9h1mx0nfou.wasm",
        "integrity": "sha256-A17yrtV2SDqY9Nb9zekYEht4cgYLHzzUYIpUY5ZHfHg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Drawing.wasm",
        "name": "System.Drawing.5zriv3afzp.wasm",
        "integrity": "sha256-ISlbf4yVD05vuW8JkMwQTWxZeE4vffu6UrTw0Kw0mzQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Dynamic.Runtime.wasm",
        "name": "System.Dynamic.Runtime.wcticfrcr3.wasm",
        "integrity": "sha256-4MJGp0G1xAiX9WLo+AqY2z/Y0NdlxK8N2fT93r40In0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Formats.Asn1.wasm",
        "name": "System.Formats.Asn1.g8s1um6b1m.wasm",
        "integrity": "sha256-MFtsVud5qdp+aEeDoT1mjXstf7z2eqVOGNfU/jaqR8A=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Globalization.wasm",
        "name": "System.Globalization.4ahc8d1dax.wasm",
        "integrity": "sha256-hHFpvRnrz0KVDXuB3CucF6BhGGx8k4AwCVXbPFubPIU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.Compression.wasm",
        "name": "System.IO.Compression.p14kqziwgr.wasm",
        "integrity": "sha256-fTdQogdHhOV8E/nFDzrwNr8lL4/+EihhOgKYUmTfNdg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.FileSystem.Primitives.wasm",
        "name": "System.IO.FileSystem.Primitives.2std312nmq.wasm",
        "integrity": "sha256-Aj/148vpT7MrsBi2RM1Qt05/0wRQQzZVHlQQXN1JE3w=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.FileSystem.Watcher.wasm",
        "name": "System.IO.FileSystem.Watcher.3luxlmz1lf.wasm",
        "integrity": "sha256-F9bKogpe5Sg3pJBR624tZNs3db/5denCIjk8y2cwT7U=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.FileSystem.wasm",
        "name": "System.IO.FileSystem.dh2a2iaujq.wasm",
        "integrity": "sha256-ERWnbp6xCQ5afHflkL+Qab+XT1TuAPtfqDQtU7V4Iq0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.MemoryMappedFiles.wasm",
        "name": "System.IO.MemoryMappedFiles.18crcv0hje.wasm",
        "integrity": "sha256-X+FBHTC+LzL3fo1DVoxoScxQ0/8KEzYthSmt6G5A/GI=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.Pipelines.wasm",
        "name": "System.IO.Pipelines.343srouqd3.wasm",
        "integrity": "sha256-w7mWkUQgx5Iv4Yve1MwE+KwPlENatHAUu+G3TKWRzLA=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.IO.wasm",
        "name": "System.IO.d1ikj8t4nt.wasm",
        "integrity": "sha256-r4Qc/HW0fV+NJqb0BCgCSdc8lCdReEWaBGgSvTLOTbU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.Expressions.wasm",
        "name": "System.Linq.Expressions.xc18wy8gwf.wasm",
        "integrity": "sha256-4SDc1IMjEAk4un48S9Nw+tW+9s5ff4hSqHWSXBK4ahU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Linq.wasm",
        "name": "System.Linq.v16hpyb3ld.wasm",
        "integrity": "sha256-HdafwVQ35MajpPBQ/NDJkV9o+3lRr4V02W5M0+GzvUc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Memory.wasm",
        "name": "System.Memory.bwm8jbq9jb.wasm",
        "integrity": "sha256-Ffu2KN9HwcjkvofkoBoSECBN0zmt1CUGBF7Z4ICoQPs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Http.Json.wasm",
        "name": "System.Net.Http.Json.tkk9rdj78c.wasm",
        "integrity": "sha256-k/AgivvFRsvv2JH7PeYgaN26FO3or/+5biWwev+2P1s=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Http.wasm",
        "name": "System.Net.Http.wb64v1mpdk.wasm",
        "integrity": "sha256-9EPLnrlbnB2HPjDmnP6ii1md78RVcJW1Rn7kpGYnKhE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Primitives.wasm",
        "name": "System.Net.Primitives.ulvkrgf47g.wasm",
        "integrity": "sha256-AggvHbvNy3hUEYh8kzlF2xX9GDhu/6rTlVsboR44O7w=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Requests.wasm",
        "name": "System.Net.Requests.jtjb1hw9iv.wasm",
        "integrity": "sha256-6QFZQ5mqypSt8dBMNINm80xABtOHp6srQe5QrynMaaw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.Security.wasm",
        "name": "System.Net.Security.0uzw9g4qnh.wasm",
        "integrity": "sha256-mNntsb2IKV6o37Bm9f1TtBtXBG9UV0mBNtADtL9ZH8c=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Net.WebHeaderCollection.wasm",
        "name": "System.Net.WebHeaderCollection.8ewaybyzhy.wasm",
        "integrity": "sha256-gO/f/dAIFZi65lsNfAph5UISAyw9tnz/xh2VxNwsMkQ=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Numerics.Vectors.wasm",
        "name": "System.Numerics.Vectors.1n4hpxfa17.wasm",
        "integrity": "sha256-vX7ZyDIuGUUqQvGZyCxjsD7ag8kpO/HoVqHeAUJ6++k=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.ObjectModel.wasm",
        "name": "System.ObjectModel.ngvo9gnv43.wasm",
        "integrity": "sha256-R872qJNb8xFq3aO/vy95yUQlbwm2rFz+uYn6CPAj1bc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Uri.wasm",
        "name": "System.Private.Uri.tfimzrj7on.wasm",
        "integrity": "sha256-2UUmm1zCwm8SLZdzjY5n+Ry+PNlntLe5QuWb9nXbRNE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Xml.Linq.wasm",
        "name": "System.Private.Xml.Linq.z6r8g29nsr.wasm",
        "integrity": "sha256-q+MsZRVyQSd+w6ro/tFPdNj6jSMLNJDeGtkmNnyF9oo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Private.Xml.wasm",
        "name": "System.Private.Xml.f9o4zv8q32.wasm",
        "integrity": "sha256-0zLEn0Wfiss/G5CNYZcRGUpC22oA8ud+l6nMo33eyio=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Extensions.wasm",
        "name": "System.Reflection.Extensions.zkbprx04ri.wasm",
        "integrity": "sha256-4yWxJzmt1l4BxlTon6224odSaU+M7N24ffThKCKDk+s=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.Metadata.wasm",
        "name": "System.Reflection.Metadata.bw7uyifa7u.wasm",
        "integrity": "sha256-e93b5q2MgWZ1KnX/mnYSQ9jSV+nwt4IeQ7r34Y7uybk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Reflection.wasm",
        "name": "System.Reflection.9o931q7mc0.wasm",
        "integrity": "sha256-R3NgzI/wZ5P94WYaakYZLUxmqxGvbe8WABSkHjgNbN0=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Resources.ResourceManager.wasm",
        "name": "System.Resources.ResourceManager.w59n8rx361.wasm",
        "integrity": "sha256-3E2dr29W8zOpX5bflzyu+B/EbAQNcAS1SI9hunkHqRo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.CompilerServices.Unsafe.wasm",
        "name": "System.Runtime.CompilerServices.Unsafe.5vixu0naqc.wasm",
        "integrity": "sha256-wauLmHyN4DVW/PIiBiYkYgxcCvHorljgg5yqfHrU3lk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Extensions.wasm",
        "name": "System.Runtime.Extensions.uqrrm5gidv.wasm",
        "integrity": "sha256-Nk+3rFhf2YFULoqqwSlxb3JpeJPkgSJ1q+sFoqRGs48=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.InteropServices.RuntimeInformation.wasm",
        "name": "System.Runtime.InteropServices.RuntimeInformation.m2wloseeld.wasm",
        "integrity": "sha256-YgJMpavmaWXPbRO9ryiJMyw3a8Ewelr0pUuLIkroPU8=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.InteropServices.wasm",
        "name": "System.Runtime.InteropServices.ph111ex6z6.wasm",
        "integrity": "sha256-MxnPK1Oq7paktuQFiClgy7xQeXf3qztY7wN4NRMpmJY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Intrinsics.wasm",
        "name": "System.Runtime.Intrinsics.2ko4h3ad8h.wasm",
        "integrity": "sha256-ri1dHKPvSUToHJidkxhXuu9Qd0Xoh0u5fOb3t0wddmE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Loader.wasm",
        "name": "System.Runtime.Loader.zssb0qqjqr.wasm",
        "integrity": "sha256-bZH2dxNSbGifJkXqxIaNJ/Lxgi4fGHcHCuY1oUvY7Rc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Numerics.wasm",
        "name": "System.Runtime.Numerics.q09oyvwf1n.wasm",
        "integrity": "sha256-Vr5Yx3cFNQen3AsMgaxXyoovt32rCyNQbbBdTxUjs+E=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.Serialization.Primitives.wasm",
        "name": "System.Runtime.Serialization.Primitives.285zczrdms.wasm",
        "integrity": "sha256-zhKp0VbgrfipYtFGRF0cjaSGw+dE6oXK1SjUsxqhp8U=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Runtime.wasm",
        "name": "System.Runtime.3h149rmxji.wasm",
        "integrity": "sha256-iYqaN8oXM63c48S65uiw+zBIhFtibcD9BbCOA9B7nnY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.AccessControl.wasm",
        "name": "System.Security.AccessControl.qq6z128xs8.wasm",
        "integrity": "sha256-4rVyX1Bpnw9ToSdcpuWfcokDLTPaiw3LMnHxKm/LImM=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Algorithms.wasm",
        "name": "System.Security.Cryptography.Algorithms.qfr6g1ruxo.wasm",
        "integrity": "sha256-mkvD3IOPZyLVdAsBbFcD+UOhvIwMHK1liDTpv2eoiHY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Cng.wasm",
        "name": "System.Security.Cryptography.Cng.f3ktysjzs0.wasm",
        "integrity": "sha256-BbPc/Y8QPOySbeQMARmkR4k+9K6ENZf5D2jKHlo8dDs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Csp.wasm",
        "name": "System.Security.Cryptography.Csp.lv9hpxz1u4.wasm",
        "integrity": "sha256-QZl3P8+JpdoukLsTEURLbkNk5CK4D51wQ0bUES3W3Eo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Encoding.wasm",
        "name": "System.Security.Cryptography.Encoding.8af9tdtphd.wasm",
        "integrity": "sha256-YKnZI7gn1sW3d7rvLrP5wF7qmqATQEnXjMJIt6GNeEE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.Primitives.wasm",
        "name": "System.Security.Cryptography.Primitives.suxq6vlbrh.wasm",
        "integrity": "sha256-HCBxVXyube7qT8yP9Q+dInyVR2uhUl+VSOe12a9BMzE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.X509Certificates.wasm",
        "name": "System.Security.Cryptography.X509Certificates.1w91kp8o9o.wasm",
        "integrity": "sha256-r/+svw4LJzDiuCM3oyF8750e3DyzoS/OXc4s4ZFmjXY=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Security.Cryptography.wasm",
        "name": "System.Security.Cryptography.5hq2j0zrn0.wasm",
        "integrity": "sha256-idWfCx62x4AFB8NAA/nxsRIxDsZPzxuBEuBNyaHctJg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Encoding.CodePages.wasm",
        "name": "System.Text.Encoding.CodePages.b3nufjuui9.wasm",
        "integrity": "sha256-mTR6wpPAe6DCRjSFBXlvILPCYCXhPq+MBKfCfD+pJUg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Encoding.Extensions.wasm",
        "name": "System.Text.Encoding.Extensions.marjb3nkfl.wasm",
        "integrity": "sha256-EqhdmjskdXq4NFLcRAoPFWph6dq+13oNbXJuaowAv54=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Encoding.wasm",
        "name": "System.Text.Encoding.kx6b7p229q.wasm",
        "integrity": "sha256-2AJx7UJbSdhxIIxVUwo/KsaEso8ASMyskwQoafu4C0s=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Encodings.Web.wasm",
        "name": "System.Text.Encodings.Web.7pqfmkh5z5.wasm",
        "integrity": "sha256-FdWti2vGbmGONLR7RxT3VYFVlES2I27FaNa05eGV9bU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.Json.wasm",
        "name": "System.Text.Json.4w11f2bdhj.wasm",
        "integrity": "sha256-1GaS3q/X8dZY99YpApjgKMml4KMESheJlvGARXt6LBw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Text.RegularExpressions.wasm",
        "name": "System.Text.RegularExpressions.w8hk38rirs.wasm",
        "integrity": "sha256-sDgtNc7Ih3qVJRSS+D4PrGhSj7IgYHkkocRM9hUGb7w=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.Tasks.wasm",
        "name": "System.Threading.Tasks.cv1rwowgwi.wasm",
        "integrity": "sha256-yD1L0SCIqN/3BqzTK5EYdQU28nFVCxwq1TbxKi3IvaU=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.Thread.wasm",
        "name": "System.Threading.Thread.94v20jm063.wasm",
        "integrity": "sha256-CZbpUIhrIWw7jX4WkICW0Eg+1t6MH6ZbGnayeCn+aZg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Threading.wasm",
        "name": "System.Threading.y0m1xpqm2i.wasm",
        "integrity": "sha256-PvULOdpBvJnttGUsBwtzWtxYJxc+TEtYBVDPTPRTfLc=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Web.HttpUtility.wasm",
        "name": "System.Web.HttpUtility.ga6sk9w62e.wasm",
        "integrity": "sha256-KBlace/Ff5L0FEaF12tUI0YqXo/nWyYCS18iTaQLyOg=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.Linq.wasm",
        "name": "System.Xml.Linq.bno46imr5t.wasm",
        "integrity": "sha256-x8MB2DkAOXhOCsJ/AzpobuVUqt727s2bp6fDdDQ1d8A=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.ReaderWriter.wasm",
        "name": "System.Xml.ReaderWriter.nt2y4g5kfd.wasm",
        "integrity": "sha256-y1bECEQ0SnkuwP4WjMy8TBGKVYAKfIZ8pYZj+N3fmV4=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.Xml.XDocument.wasm",
        "name": "System.Xml.XDocument.9wrwyiyftk.wasm",
        "integrity": "sha256-x+Be558FFtu3CEezZ5CBQOiBDTFlTvOeF0TCOMYR1zs=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "System.wasm",
        "name": "System.l9jolnojrw.wasm",
        "integrity": "sha256-d1dcH2nxptL8QLqZqtRI5G+lrjrbUKrUOnC/yloVB98=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "mscorlib.wasm",
        "name": "mscorlib.ntskz0lhc2.wasm",
        "integrity": "sha256-62Hy5vVSsjpdBTtC7Jea38ZGsAwF97OpSKCfGythgIo=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "netstandard.wasm",
        "name": "netstandard.ywed71ik29.wasm",
        "integrity": "sha256-PkiWBGe8b8victmAP0NHpZz8QW2OvMNxDPfrkXE5IPw=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Demo.Abstractions.wasm",
        "name": "Demo.Abstractions.36ib0a3g6x.wasm",
        "integrity": "sha256-u8it7sR+8AdJn098safF9tWw8HW6IHHMwi7o/bLEybE=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "GeneticAlgorithm.wasm",
        "name": "GeneticAlgorithm.q6vf4lhh1c.wasm",
        "integrity": "sha256-XmwbnCiVjg/ANZqA2spBNbYfvwsnaE9Pz+8+DfV7Imk=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "Demo.wasm",
        "name": "Demo.f01eudon8c.wasm",
        "integrity": "sha256-n25+itvrMlYygPvSqJ6aElT13TLZGVraLoxXzOdc2PQ=",
        "cache": "force-cache"
      }
    ],
    "lazyAssembly": [
      {
        "virtualPath": "MarkdownToWordDemo.wasm",
        "name": "MarkdownToWordDemo.qqr4568xnh.wasm",
        "integrity": "sha256-ciI9dNcRklptsZcpY2ITCMXBaNGy5tbsSmk8WoEzU7Q=",
        "cache": "force-cache"
      },
      {
        "virtualPath": "QrTransferDemo.wasm",
        "name": "QrTransferDemo.bs9m8hqjir.wasm",
        "integrity": "sha256-9dip76ThAVO+isyhG71XpmlfxcW3TbfsQvrhCINtdB4=",
        "cache": "force-cache"
      }
    ]
  },
  "debugLevel": 0,
  "linkerEnabled": true,
  "globalizationMode": "sharded",
  "extensions": {
    "blazor": {}
  },
  "runtimeConfig": {
    "runtimeOptions": {
      "configProperties": {
        "Microsoft.AspNetCore.Components.Routing.RegexConstraintSupport": false,
        "Microsoft.Extensions.DependencyInjection.VerifyOpenGenericServiceTrimmability": true,
        "System.ComponentModel.DefaultValueAttribute.IsSupported": false,
        "System.ComponentModel.Design.IDesignerHost.IsSupported": false,
        "System.ComponentModel.TypeConverter.EnableUnsafeBinaryFormatterInDesigntimeLicenseContextSerialization": false,
        "System.ComponentModel.TypeDescriptor.IsComObjectDescriptorSupported": false,
        "System.Data.DataSet.XmlSerializationIsSupported": false,
        "System.Diagnostics.Debugger.IsSupported": false,
        "System.Diagnostics.Metrics.Meter.IsSupported": false,
        "System.Diagnostics.Tracing.EventSource.IsSupported": false,
        "System.GC.Server": true,
        "System.Globalization.Invariant": false,
        "System.TimeZoneInfo.Invariant": false,
        "System.Linq.Enumerable.IsSizeOptimized": true,
        "System.Net.Http.EnableActivityPropagation": false,
        "System.Net.Http.WasmEnableStreamingResponse": true,
        "System.Net.SocketsHttpHandler.Http3Support": false,
        "System.Reflection.Metadata.MetadataUpdater.IsSupported": false,
        "System.Resources.ResourceManager.AllowCustomResourceTypes": false,
        "System.Resources.UseSystemResourceKeys": true,
        "System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported": true,
        "System.Runtime.InteropServices.BuiltInComInterop.IsSupported": false,
        "System.Runtime.InteropServices.EnableConsumingManagedCodeFromNativeHosting": false,
        "System.Runtime.InteropServices.EnableCppCLIHostActivation": false,
        "System.Runtime.InteropServices.Marshalling.EnableGeneratedComInterfaceComImportInterop": false,
        "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false,
        "System.StartupHookProvider.IsSupported": false,
        "System.Text.Encoding.EnableUnsafeUTF7Encoding": false,
        "System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault": true,
        "System.Threading.Thread.EnableAutoreleasePool": false,
        "Microsoft.AspNetCore.Components.Endpoints.NavigationManager.DisableThrowNavigationException": false
      }
    }
  }
}/*json-end*/);export{gt as default,ft as dotnet,mt as exit};
