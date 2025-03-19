import { MessageApi, DialogApi, NotificationApi } from "naive-ui";

declare global {
  interface Window {
    $message: MessageApi;
    $dialog: DialogApi;
    $notify: NotificationApi;
  }
}

export {}
