import {Stack, NavLink} from '@mantine/core'
import {Link, useRouterState} from '@tanstack/react-router'
import {Home, ClipboardType} from 'lucide-react'

interface NavigationProps {
    onNavigate?: () => void
}

export default function Navigation({onNavigate}: NavigationProps) {
    const router = useRouterState()
    const currentPath = router.location.pathname

    return (
        <Stack gap="xs">
            <Link to="/" style={{textDecoration: 'none', color: 'inherit'}}>
                <NavLink
                    component={'span'}
                    label="Home"
                    leftSection={<Home size={20}/>}
                    active={currentPath === '/'}
                    onClick={onNavigate}
                />
            </Link>
            <Link to="/demo/form/simple" style={{textDecoration: 'none', color: 'inherit'}}>
                <NavLink
                    component={'span'}
                    label="Simple Form"
                    leftSection={<ClipboardType size={20}/>}
                    active={currentPath === '/demo/form/simple'}
                    onClick={onNavigate}
                />
            </Link>
            <Link to="/demo/form/address" style={{textDecoration: 'none', color: 'inherit'}}>
                <NavLink
                    component={'span'}
                    label="Address Form"
                    leftSection={<ClipboardType size={20}/>}
                    active={currentPath === '/demo/form/address'}
                    onClick={onNavigate}
                />
            </Link>
        </Stack>
    )
}

