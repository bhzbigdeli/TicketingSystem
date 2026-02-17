function App() {
  const {
    Alert,
    Button,
    Card,
    Checkbox,
    Flex,
    Form,
    Input,
    Typography,
  } = window.antd ?? {};

  if (!window.antd) {
    return (
      <main className="app-shell">
        <section className="app-fallback">
          <h1>Loading Ant Design…</h1>
          <p>If this message persists, check your internet connection.</p>
        </section>
      </main>
    );
  }

  return (
    <main className="app-shell">
      <Card className="login-card" bordered={false}>
        <Flex vertical gap={20}>
          <Typography.Title level={2} style={{ margin: 0 }}>
            Ticketing System Login
          </Typography.Title>
          <Typography.Paragraph type="secondary" style={{ margin: 0 }}>
            Sign in to manage tickets, users, and support queues.
          </Typography.Paragraph>

          <Form
            layout="vertical"
            name="login"
            requiredMark={false}
            initialValues={{ remember: true }}
            onFinish={(values) => {
              window.alert(`Welcome back, ${values.email}!`);
            }}
          >
            <Form.Item
              label="Email"
              name="email"
              rules={[
                { required: true, message: 'Please enter your email address.' },
                { type: 'email', message: 'Please enter a valid email address.' },
              ]}
            >
              <Input size="large" placeholder="agent@ticketing.local" />
            </Form.Item>

            <Form.Item
              label="Password"
              name="password"
              rules={[{ required: true, message: 'Please enter your password.' }]}
            >
              <Input.Password size="large" placeholder="Enter password" />
            </Form.Item>

            <Flex justify="space-between" align="center" style={{ marginBottom: 16 }}>
              <Form.Item name="remember" valuePropName="checked" noStyle>
                <Checkbox>Remember me</Checkbox>
              </Form.Item>
              <Button type="link" style={{ paddingInline: 0 }}>
                Forgot password?
              </Button>
            </Flex>

            <Button type="primary" htmlType="submit" size="large" block>
              Log in
            </Button>
          </Form>

          <Alert
            type="info"
            showIcon
            message="Demo login"
            description="Use any valid email format and a password to submit the form."
          />
        </Flex>
      </Card>
    </main>
  );
}

export default App;
