
let client=null;

export function initSseClient() {
  if(client!==null) return;

  client=new EventSource('/api/sse');

  return client;


}


