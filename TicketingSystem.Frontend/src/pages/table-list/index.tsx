import type { ActionType, ProColumns } from '@ant-design/pro-components';
import {
  ModalForm,
  PageContainer,
  ProFormSelect,
  ProFormText,
  ProFormTextArea,
  ProTable,
} from '@ant-design/pro-components';
import { Button, Popconfirm, Tag, message } from 'antd';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import {
  changeTicketStatus,
  createTicket,
  deleteTicket,
  getTickets,
  getUsers,
  type TicketDto,
  type TicketPriority,
  type TicketStatus,
  updateTicket,
  type UserDto,
} from '@/services/ticketing';

const priorityMap: Record<TicketPriority, { text: string; color: string }> = {
  1: { text: 'Low', color: 'default' },
  2: { text: 'Medium', color: 'blue' },
  3: { text: 'High', color: 'orange' },
  4: { text: 'Critical', color: 'red' },
};

const statusMap: Record<TicketStatus, { text: string; color: string }> = {
  1: { text: 'Open', color: 'blue' },
  2: { text: 'In Progress', color: 'gold' },
  3: { text: 'Resolved', color: 'green' },
  4: { text: 'Closed', color: 'default' },
};

const TableList: React.FC = () => {
  const actionRef = useRef<ActionType | null>(null);
  const [messageApi, contextHolder] = message.useMessage();
  const [users, setUsers] = useState<UserDto[]>([]);
  const [editingTicket, setEditingTicket] = useState<TicketDto | undefined>(undefined);

  useEffect(() => {
    getUsers()
      .then(setUsers)
      .catch(() => messageApi.error('Failed to load users from API'));
  }, [messageApi]);

  const userOptions = useMemo(
    () => users.map((user) => ({ label: `${user.fullName} (${user.email})`, value: user.id })),
    [users],
  );

  const userNameById = useMemo(
    () => new Map(users.map((user) => [user.id, user.fullName])),
    [users],
  );

  const columns: ProColumns<TicketDto>[] = [
    {
      title: 'Title',
      dataIndex: 'title',
    },
    {
      title: 'Priority',
      dataIndex: 'priority',
      render: (_, record) => <Tag color={priorityMap[record.priority].color}>{priorityMap[record.priority].text}</Tag>,
      filters: true,
      onFilter: true,
      valueEnum: {
        1: { text: 'Low' },
        2: { text: 'Medium' },
        3: { text: 'High' },
        4: { text: 'Critical' },
      },
    },
    {
      title: 'Status',
      dataIndex: 'status',
      render: (_, record) => <Tag color={statusMap[record.status].color}>{statusMap[record.status].text}</Tag>,
      valueEnum: {
        1: { text: 'Open' },
        2: { text: 'In Progress' },
        3: { text: 'Resolved' },
        4: { text: 'Closed' },
      },
    },
    {
      title: 'Created By',
      dataIndex: 'createdById',
      render: (_, record) => userNameById.get(record.createdById) || record.createdById,
    },
    {
      title: 'Created At',
      dataIndex: 'createdAt',
      valueType: 'dateTime',
    },
    {
      title: 'Actions',
      valueType: 'option',
      render: (_, record) => [
        <a key="edit" onClick={() => setEditingTicket(record)}>
          Edit
        </a>,
        <a
          key="nextStatus"
          onClick={async () => {
            const nextStatus = (record.status === 4 ? 1 : (record.status + 1)) as TicketStatus;
            await changeTicketStatus(record.id, nextStatus);
            messageApi.success('Ticket status updated');
            actionRef.current?.reload();
          }}
        >
          Next status
        </a>,
        <Popconfirm
          key="delete"
          title="Delete this ticket?"
          onConfirm={async () => {
            await deleteTicket(record.id);
            messageApi.success('Ticket deleted');
            actionRef.current?.reload();
          }}
        >
          <a>Delete</a>
        </Popconfirm>,
      ],
    },
  ];

  return (
    <PageContainer title="Ticketing API Integration">
      {contextHolder}
      <ProTable<TicketDto>
        actionRef={actionRef}
        rowKey="id"
        search={false}
        columns={columns}
        request={async (params) => {
          const data = await getTickets({
            page: params.current,
            pageSize: params.pageSize,
          });

          return {
            data,
            success: true,
            total: data.length,
          };
        }}
        toolBarRender={() => [
          <ModalForm
            key="create"
            title="Create ticket"
            trigger={<Button type="primary">New ticket</Button>}
            onFinish={async (values) => {
              await createTicket({
                title: values.title,
                description: values.description,
                priority: values.priority,
                createdById: values.createdById,
              });

              messageApi.success('Ticket created');
              actionRef.current?.reload();
              return true;
            }}
          >
            <ProFormText name="title" label="Title" rules={[{ required: true, min: 3 }]} />
            <ProFormTextArea
              name="description"
              label="Description"
              rules={[{ required: true, min: 3 }]}
            />
            <ProFormSelect
              name="priority"
              label="Priority"
              options={[
                { label: 'Low', value: 1 },
                { label: 'Medium', value: 2 },
                { label: 'High', value: 3 },
                { label: 'Critical', value: 4 },
              ]}
              rules={[{ required: true }]}
            />
            <ProFormSelect
              name="createdById"
              label="Created By"
              options={userOptions}
              rules={[{ required: true, message: 'Create a user in API first, then select it here.' }]}
            />
          </ModalForm>,
        ]}
      />

      <ModalForm
        title="Edit ticket"
        open={Boolean(editingTicket)}
        onOpenChange={(open) => {
          if (!open) {
            setEditingTicket(undefined);
          }
        }}
        initialValues={editingTicket}
        onFinish={async (values) => {
          if (!editingTicket) {
            return false;
          }

          await updateTicket(editingTicket.id, {
            title: values.title,
            description: values.description,
            priority: values.priority,
          });

          messageApi.success('Ticket updated');
          setEditingTicket(undefined);
          actionRef.current?.reload();
          return true;
        }}
      >
        <ProFormText name="title" label="Title" rules={[{ required: true, min: 3 }]} />
        <ProFormTextArea name="description" label="Description" rules={[{ required: true, min: 3 }]} />
        <ProFormSelect
          name="priority"
          label="Priority"
          options={[
            { label: 'Low', value: 1 },
            { label: 'Medium', value: 2 },
            { label: 'High', value: 3 },
            { label: 'Critical', value: 4 },
          ]}
          rules={[{ required: true }]}
        />
      </ModalForm>
    </PageContainer>
  );
};

export default TableList;