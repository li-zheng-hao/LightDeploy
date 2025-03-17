<template>
  <div class="p-4">
    <n-card>
      <n-space vertical :size="12">
        <!-- 搜索区域使用Tailwind CSS -->
        <div class="flex flex-wrap items-center gap-4">
          <div class="flex items-center">
            <span class="min-w-[60px] text-left mr-2">环境：</span>
            <n-input
              v-model:value="searchForm.environment"
              placeholder="请输入环境"
              clearable
            />
          </div>
          <div class="flex items-center">
            <span class="min-w-[80px] text-left mr-2">服务分组：</span>
            <n-input
              v-model:value="searchForm.groupName"
              placeholder="请输入服务分组"
              clearable
            />
          </div>
          <div class="flex items-center">
            <span class="min-w-[80px] text-left mr-2">服务名称：</span>
            <n-input
              v-model:value="searchForm.serviceName"
              placeholder="请输入服务名称"
              clearable
            />
          </div>
          <div class="flex items-center">
            <n-button type="primary" @click="handleSearch">
              <template #icon>
                <n-icon>
                  <Search />
                </n-icon>
              </template>
              搜索
            </n-button>
          </div>
          <div class="flex items-center">
            <n-button type="primary" @click="handleAdd">
              <template #icon>
                <n-icon>
                  <Add />
                </n-icon>
              </template>
              新增服务
            </n-button>
          </div>
        </div>

        <!-- 表格区域 -->
        <n-data-table
          :columns="columns"
          :data="filteredData"
          :pagination="pagination"
          :bordered="false"
          striped
        />
      </n-space>
    </n-card>

    <!-- 编辑/新增对话框 -->
    <n-modal
      v-model:show="showModal"
      :mask-closable="false"
      preset="dialog"
      :title="modalTitle"
    >
      <n-form
        ref="formRef"
        :model="formData"
        :rules="rules"
        label-placement="left"
        label-width="120"
        require-mark-placement="right-hanging"
      >
        <n-form-item label="环境" path="environment">
          <n-input
            v-model:value="formData.environment"
            placeholder="请输入环境"
          />
        </n-form-item>
        <n-form-item label="服务分组" path="groupName">
          <n-input
            v-model:value="formData.groupName"
            placeholder="请输入服务分组"
          />
        </n-form-item>
        <n-form-item label="服务名称" path="serviceName">
          <n-input
            v-model:value="formData.serviceName"
            placeholder="请输入服务名称"
          />
        </n-form-item>
        <n-form-item label="项目路径" path="projectPath">
          <n-input
            v-model:value="formData.projectPath"
            placeholder="请输入项目路径"
          />
        </n-form-item>
        <n-form-item label="端口" path="port">
          <n-input-number
            v-model:value="formData.port"
            placeholder="请输入端口号"
          />
        </n-form-item>
        <n-form-item label="项目类型" path="projectType">
          <n-select
            v-model:value="formData.projectType"
            :options="ProjectTypeOptions"
          />
        </n-form-item>
        <n-form-item label="备注" path="comment">
          <n-input
            v-model:value="formData.comment"
            type="textarea"
            placeholder="请输入备注"
          />
        </n-form-item>
        <n-form-item label="仅复制文件" path="onlyCopyFile">
          <n-checkbox v-model:checked="formData.onlyCopyFile"
            >仅复制文件</n-checkbox
          >
        </n-form-item>
      </n-form>
      <template #action>
        <n-space>
          <n-button @click="showModal = false">取消</n-button>
          <n-button type="primary" @click="handleSubmit">确定</n-button>
        </n-space>
      </template>
    </n-modal>

    <!-- 发布目标对话框 -->
    <n-modal
      v-model:show="showTargetModal"
      :mask-closable="true"
      style="width: 900px"
    >
      <n-card :title="targetModalTitle">
        <template #header-extra>
          <n-button type="primary" @click="handleAddTarget">
            <template #icon>
              <n-icon>
                <Add />
              </n-icon>
            </template>
            新增目标
          </n-button>
        </template>

        <!-- 目标列表表格 -->
        <n-data-table
          :columns="targetColumns"
          :data="targetTableData"
          :bordered="false"
          striped
        />

        <!-- 目标表单对话框 -->
        <n-modal
          v-model:show="showTargetFormModal"
          :mask-closable="false"
          preset="dialog"
          :title="targetFormTitle"
        >
          <n-form
            ref="targetFormRef"
            :model="targetFormData"
            :rules="targetRules"
            label-placement="left"
            label-width="120"
            require-mark-placement="right-hanging"
          >
            <n-form-item label="主机" path="host">
              <n-input
                v-model:value="targetFormData.host"
                placeholder="请输入目标主机"
              />
            </n-form-item>
            <n-form-item label="端口" path="port">
              <n-input-number
                v-model:value="targetFormData.port"
                placeholder="请输入端口号"
              />
            </n-form-item>
            <n-form-item label="服务路径" path="servicePath">
              <n-input
                v-model:value="targetFormData.servicePath"
                placeholder="请输入目标服务器服务所在文件夹"
              />
            </n-form-item>
            <n-form-item label="程序执行路径" path="exePath">
              <n-input
                v-model:value="targetFormData.exePath"
                placeholder="请输入服务程序执行完整路径"
              />
            </n-form-item>
            <n-form-item label="程序执行参数" path="exeParams">
              <n-input
                v-model:value="targetFormData.exeParams"
                placeholder="请输入服务程序执行参数"
              />
            </n-form-item>
            <n-form-item label="备注" path="comment">
              <n-input
                v-model:value="targetFormData.comment"
                type="textarea"
                placeholder="请输入备注"
              />
            </n-form-item>
          </n-form>
          <template #action>
            <n-space>
              <n-button @click="showTargetFormModal = false">取消</n-button>
              <n-button type="primary" @click="handleSubmitTarget"
                >确定</n-button
              >
            </n-space>
          </template>
        </n-modal>
      </n-card>
    </n-modal>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, h, onMounted } from "vue";
import { Search, Add } from "@vicons/ionicons5";
import type { DataTableColumns } from "naive-ui";
import { useMessage } from "naive-ui";
import type { FormInst } from "naive-ui";
import {
  getServiceList,
  createDeployService,
  updateDeployService,
  deleteDeployService,
  type DeployService,
  ProjectTypeOptions,
} from "../api/service";
import {
  getTargetList,
  createDeployTarget,
  updateDeployTarget,
  deleteDeployTarget,
  type DeployTarget,
} from "../api/target";
import { NButton } from "naive-ui";

const message = useMessage();

// 搜索表单
const searchForm = ref({
  environment: "",
  groupName: "",
  serviceName: "",
});

// 表格数据
const tableData = ref<DeployService[]>([]);

// 过滤后的数据
const filteredData = computed(() => {
  if (!tableData.value) return [];
  return tableData.value.filter((item) => {
    const environmentMatch = item.environment
      ?.toLowerCase()
      .includes(searchForm.value.environment.toLowerCase());
    const groupNameMatch = item.groupName
      .toLowerCase()
      .includes(searchForm.value.groupName.toLowerCase());
    const serviceNameMatch = item.serviceName
      .toLowerCase()
      .includes(searchForm.value.serviceName.toLowerCase());
    return environmentMatch && groupNameMatch && serviceNameMatch;
  });
});

// 分页配置
const pagination = ref({
  page: 1,
  pageSize: 10,
  showSizePicker: true,
  pageSizes: [10, 20, 30, 40],
  onChange: (page: number) => {
    pagination.value.page = page;
  },
  onUpdatePageSize: (pageSize: number) => {
    pagination.value.pageSize = pageSize;
    pagination.value.page = 1;
  },
});

// 表格列定义
const columns: DataTableColumns<DeployService> = [
  { title: "环境", key: "environment" },
  { title: "服务分组", key: "groupName" },
  { title: "服务名称", key: "serviceName" },
  { title: "项目路径", key: "projectPath" },
  { title: "端口", key: "port" },
  {
    title: "仅复制文件",
    key: "onlyCopyFile",
    render(row) {
      return row.onlyCopyFile ? "是" : "否";
    },
  },
  {
    title: "项目类型",
    key: "projectType",
    render(row) {
      const option = ProjectTypeOptions.find(
        (opt) => opt.value === row.projectType
      );
      return option ? option.label : "未知";
    },
  },
  { title: "备注", key: "comment" },
  {
    title: "操作",
    key: "actions",
    render(row) {
      return h("div", [
        h(
          NButton,
          {
            size: "small",
            quaternary: true,
            type: "info",
            onClick: () => handleEdit(row),
          },
          {
            default: () => "编辑",
          }
        ),
        h(
          NButton,
          {
            size: "small",
            quaternary: true,
            type: "info",
            onClick: () => handleConfigTarget(row),
          },
          {
            default: () => "配置发布目标",
          }
        ),
        h(
          NButton,
          {
            size: "small",
            quaternary: true,
            type: "error",
            onClick: () => handleDelete(row),
          },
          {
            default: () => "删除",
          }
        ),
      ]);
    },
  },
];

// 表单相关
const showModal = ref(false);
const modalTitle = ref("");
const formRef = ref<FormInst | null>(null);
const formData = ref<Partial<DeployService>>({
  groupName: "",
  serviceName: "",
  projectPath: "",
  port: undefined,
  projectType: undefined,
  comment: "",
  onlyCopyFile: false,
});

// 表单校验规则
const rules = {
  environment: {
    required: true,
    message: "请输入环境",
    trigger: "blur",
  },
  groupName: {
    required: true,
    message: "请输入服务分组",
    trigger: "blur",
  },
  serviceName: {
    required: true,
    message: "请输入服务名称",
    trigger: "blur",
  },
  projectPath: {
    required: true,
    message: "请输入项目路径",
    trigger: "blur",
  },
  port: {
    required: true,
    message: "请输入端口号（0-65535之间）",
    trigger: "blur",
    type: "number",
    validator: (_rule: any, value: number) => {
      if (value < 0 || value > 65535) {
        return new Error("端口号必须在0-65535之间");
      }
      return true;
    },
  },
  projectType: {
    required: true,
    message: "请选择项目类型",
    trigger: "change",
    type: "number",
  },
};

// 发布目标相关
const showTargetModal = ref(false);
const showTargetFormModal = ref(false);
const targetModalTitle = ref("发布目标配置");
const targetFormTitle = ref("");
const selectedService = ref<DeployService | null>(null);
const targetFormRef = ref<FormInst | null>(null);
const targetFormData = ref<Partial<DeployTarget>>({
  host: "",
  port: undefined,
  secretKey: "",
  servicePath: "",
  comment: "",
});
const targetTableData = ref<DeployTarget[]>([]);

// 发布目标表单校验规则
const targetRules = {
  host: {
    required: true,
    message: "请输入目标主机",
    trigger: "blur",
  },
  port: {
    required: true,
    message: "请输入端口号",
    trigger: "blur",
    type: "number",
    validator: (_rule: any, value: number) => {
      if (value < 0 || value > 65535) {
        return new Error("端口号必须在0-65535之间");
      }
      return true;
    },
  },
  servicePath: {
    required: true,
    message: "请输入服务路径",
    trigger: "blur",
  },
};

// 发布目标表格列定义
const targetColumns: DataTableColumns<DeployTarget> = [
  { title: "主机", key: "host" },
  { title: "端口", key: "port" },
  { title: "服务路径", key: "servicePath" },
  { title: "备注", key: "comment" },
  {
    title: "操作",
    key: "actions",
    render(row) {
      return h("div", [
        h(
          NButton,
          {
            size: "small",
            quaternary: true,
            type: "info",
            onClick: () => handleEditTarget(row),
          },
          {
            default: () => "编辑",
          }
        ),
        h(
          NButton,
          {
            size: "small",
            quaternary: true,
            type: "error",
            onClick: () => handleDeleteTarget(row),
          },
          {
            default: () => "删除",
          }
        ),
      ]);
    },
  },
];

// 初始化数据
onMounted(async () => {
  await loadData();
});

// 加载数据
const loadData = async () => {
  try {
    const response = await getServiceList();
    tableData.value = response.data || [];
  } catch (error) {
    tableData.value = [];
    message.error("加载数据失败");
  }
};

// 搜索
const handleSearch = () => {
  pagination.value.page = 1;
};

// 新增
const handleAdd = () => {
  modalTitle.value = "新增服务";
  formData.value = {
    groupName: "",
    serviceName: "",
    projectPath: "",
    port: undefined,
    projectType: undefined,
    comment: "",
  };
  showModal.value = true;
};

// 编辑
const handleEdit = (row: DeployService) => {
  modalTitle.value = "编辑服务";
  formData.value = { ...row };
  showModal.value = true;
};

// 删除
const handleDelete = async (row: DeployService) => {
  try {
    await deleteDeployService(row.id);
    message.success("删除成功");
    await loadData();
  } catch (error) {
    message.error("删除失败");
  }
};

// 提交表单
const handleSubmit = async () => {
  if (!formRef.value) return;

  try {
    await formRef.value.validate();
    if (formData.value.id) {
      // 编辑
      await updateDeployService(formData.value as DeployService);
      message.success("更新成功");
    } else {
      // 新增
      await createDeployService(formData.value as Omit<DeployService, "id">);
      message.success("创建成功");
    }
    showModal.value = false;
    await loadData();
  } catch (error) {
    message.error("提交失败");
  }
};

// 加载发布目标数据
const loadTargetData = async (serviceId: number) => {
  try {
    const response = await getTargetList(serviceId);
    targetTableData.value = response.data || [];
  } catch (error) {
    targetTableData.value = [];
    message.error("加载发布目标数据失败");
  }
};

// 配置发布目标
const handleConfigTarget = async (row: DeployService) => {
  selectedService.value = row;
  showTargetModal.value = true;
  await loadTargetData(row.id);
};

// 新增发布目标
const handleAddTarget = () => {
  targetFormTitle.value = "新增发布目标";
  targetFormData.value = {
    host: "",
    port: undefined,
    secretKey: "",
    servicePath: "",
    comment: "",
  };
  showTargetFormModal.value = true;
};

// 编辑发布目标
const handleEditTarget = (row: DeployTarget) => {
  targetFormTitle.value = "编辑发布目标";
  targetFormData.value = { ...row };
  showTargetFormModal.value = true;
};

// 删除发布目标
const handleDeleteTarget = async (row: DeployTarget) => {
  try {
    await deleteDeployTarget(row.id);
    message.success("删除成功");
    if (selectedService.value) {
      await loadTargetData(selectedService.value.id);
    }
  } catch (error) {
    message.error("删除失败");
  }
};

// 提交发布目标表单
const handleSubmitTarget = async () => {
  if (!targetFormRef.value || !selectedService.value) return;

  try {
    await targetFormRef.value.validate();
    const submitData = {
      ...targetFormData.value,
      serviceId: selectedService.value.id,
    };

    if ("id" in targetFormData.value) {
      // 编辑
      await updateDeployTarget(submitData as DeployTarget);
      message.success("更新成功");
    } else {
      // 新增
      await createDeployTarget(submitData as Omit<DeployTarget, "id">);
      message.success("创建成功");
    }

    showTargetFormModal.value = false;
    await loadTargetData(selectedService.value.id);
    targetFormData.value = {};
  } catch (error) {
    message.error("提交失败");
  }
};
</script>

<style scoped>
.service-config {
  padding: 16px;
}

/* 添加新的样式 */
.search-area {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  align-items: center;
}

.search-item {
  display: flex;
  align-items: center;
}

.search-item .label {
  min-width: 80px;
  text-align: left;
  margin-right: 8px;
}
</style>
