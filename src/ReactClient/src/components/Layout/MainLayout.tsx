import {Link, Outlet} from "react-router-dom";
import {useAuth} from "../../providers/AuthProvider.tsx";

const navigation = [
    {name: 'Features', href: '/features', current: false},
    {name: 'Docs', href: 'https://aufy.dev/docs/introduction', current: false},
]

const userNavigation = [
    {name: 'My Account', href: '/profile', requireAuth: true},
    {name: 'Sign out', href: '/signout', requireAuth: true},
    {name: 'Sign in', href: '/signin', requireAuth: false},
]

function classNames({classes}: { classes?: string[] }) {
    return classes ? classes.filter(Boolean).join(' ') : ' '
}

export default function MainLayout() {
    const {user} = useAuth();

    return (
        <>
            <div className="min-h-full">
                <nav className="border-b border-gray-200 bg-white">
                    <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
                        <div className="flex h-16 justify-between">
                            <div className="flex">
                                <Link to={"/"} className="flex flex-shrink-0 items-center ">
                                    <h1 className="font-bold primary-color hover:primary-color">Aufy 💚 React</h1>
                                </Link>
                                <div className="-my-px ml-6 flex space-x-8">
                                    {navigation.map((item) => (
                                        <Link
                                            key={item.name}
                                            to={item.href}
                                            className={classNames(
                                                {
                                                    classes: [item.current
                                                        ? 'border-indigo-500 text-gray-900'
                                                        : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700', 'inline-flex items-center border-b-2 px-1 pt-1 text-sm font-medium']
                                                }
                                            )}
                                            aria-current={item.current ? 'page' : undefined}
                                        >
                                            {item.name}
                                        </Link>
                                    ))}
                                </div>
                            </div>
                            <div className="ml-6 flex items-center">
                                {userNavigation.map((item) => item.requireAuth === !!user &&
                                    <Link
                                        to={item.href}
                                        className='cursor-pointer block px-4 py-2 text-sm text-gray-700'
                                    >
                                        {item.name} 
                                    </Link>
                                )}
                            </div>
                        </div>
                    </div>
                </nav>

                <div className="py-10">
                    <main>
                        <div className="mx-auto max-w-7xl sm:px-6 lg:px-8">
                            <Outlet/>
                        </div>
                    </main>
                </div>
            </div>
        </>
    )
}
