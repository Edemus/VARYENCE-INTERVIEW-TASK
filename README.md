TASK DESCRIPTION
Implement local network analysis tool. This tool should be able to detect machines in local
network, validate if ports 1000 and 3389 are open (there is an application that listens this port)
and report this remote server using HTTP post request

REQUIREMENTS
- When application launches it should get list of machines within local network
- Once there is a list of machines, application should iterate over each machine and check
if there is an application listening on port 1000 and 3389
- Once it collected all required information it should post it to: ‘https://this-is-not-validlink.com/varyence-test’
