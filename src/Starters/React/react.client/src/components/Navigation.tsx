import { Title, Stack, NavLink } from '@mantine/core'
import { useRouterState } from '@tanstack/react-router'
import { Home, ClipboardType } from 'lucide-react'

interface NavigationProps {
  onNavigate?: () => void
}

export default function Navigation({ onNavigate }: NavigationProps) {
  const router = useRouterState()
  const currentPath = router.location.pathname

  return (
    <Stack gap="xs">
      <NavLink
        href="/"
        label="Home"
        leftSection={<Home size={20} />}
        active={currentPath === '/'}
        onClick={onNavigate}
      />
      <NavLink
        href="/demo/form/simple"
        label="Simple Form"
        leftSection={<ClipboardType size={20} />}
        active={currentPath === '/demo/form/simple'}
        onClick={onNavigate}
      />
      <NavLink
        href="/demo/form/address"
        label="Address Form"
        leftSection={<ClipboardType size={20} />}
        active={currentPath === '/demo/form/address'}
        onClick={onNavigate}
      />
    </Stack>
  )
}

