import { createFileRoute } from '@tanstack/react-router'
import { Container, Title, Text, Button, Stack, Group, Anchor } from '@mantine/core'
import logo from '../logo.svg'

export const Route = createFileRoute('/')({
  component: App,
})

function App() {
  return (
    <Container size="md" py="xl">
      <Stack align="center" gap="xl" mt="xl">
        <img
          src={logo}
          style={{
            height: '40vmin',
            pointerEvents: 'none',
            animation: 'spin 20s linear infinite',
          }}
          alt="logo"
        />
        <Title order={1} ta="center">
          Welcome to React + TanStack Router
        </Title>
        <Text size="lg" c="dimmed" ta="center">
          Edit <code>src/routes/index.tsx</code> and save to reload.
        </Text>
        <Group gap="md">
          <Button
            component="a"
            href="https://reactjs.org"
            target="_blank"
            rel="noopener noreferrer"
            variant="light"
          >
            Learn React
          </Button>
          <Button
            component="a"
            href="https://tanstack.com"
            target="_blank"
            rel="noopener noreferrer"
            variant="light"
          >
            Learn TanStack
          </Button>
        </Group>
        <Text size="sm" c="dimmed" ta="center" mt="md">
          This project is now using{' '}
          <Anchor href="https://mantine.dev" target="_blank" rel="noopener noreferrer">
            Mantine UI
          </Anchor>{' '}
          for beautiful components!
        </Text>
      </Stack>
    </Container>
  )
}
