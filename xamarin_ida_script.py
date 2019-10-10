import idautils
import idc
import collections

searchItems = {"mono_init_internal" : "Max native code in a domain", #req'd for iOS
               "mono_class_load_from_name" : "Runtime critical type %s.%s not found", # req'd for iOS
               "mono_assembly_request_open" : "Assembly Loader probing location: '%s'.", # req'd for iOS
               "mono_class_init_internal" : "Recursive type definition detected", # req'd for iOS
               "mono_class_setup_methods" : "Could not load method %d due to %s",
               "mono_class_get_method_by_index" : "index >= 0 && index < mono_class_get_method_count (klass)", # req'd for iOS
               "mono_thread_attach" : "Handle Stack",
               # for iOS only
               "mono_aot_get_method" : "AOT NOT FOUND: %s.", #req'd for iOS
               "mono_aot_get_class_from_name" : "%s.%s",
               # for Android only
               # https://mono.github.io/mail-archives/mono-list/2004-January/017584.html - do a substring search only
               "mono_jit_compile_method_with_opt" : "Attempting to JIT compile method '%s' while running in aot-only mode.",
               "mini_init" : "Mono requires /proc to be mounted.\n",
              }

stringList = idautils.Strings()
output = []
match = False

for function, text in searchItems.iteritems():
    for s in stringList:
        if (str(function) == 'mono_jit_compile_method_with_opt') and (str(text) in str(s)):
            match = True
        elif str(text) == str(s):
            match = True
        if match:
            print('\n[+] Located string "{}" at addresss 0x{:x}'.format(str(s), s.ea))
            xrefs = idautils.XrefsTo(s.ea)
            funcAdds = []
            for xref in xrefs:
                # FUNCATTR_START   =  0     # readonly: function start address
                funcAdds.append(idc.get_func_attr(xref.frm, 0))
                #print('[debug] {}'.format(idc.get_func_attr(funcAdds[-1], 0x20)))
            # test for mono_aot_get_class_from_name
            if str(function) == 'mono_aot_get_class_from_name':
                # string is xref'd in the same function of interest exactly twice
                funcAdds = [x for x, y in collections.Counter(funcAdds).items() if y == 2]
                # TODO: add another check for '%s' in the identified functions. this should narrow the candidates down
            elif str(function) == 'mono_thread_attach':
                # Both 'Handle Stack' and 'Thread Stack' are present
                print('[DEBUG] mono_thread_attach')
                for w in stringList:
                    if str('Thread Stack') in str(w):
                        ts_xrefs = idautils.XrefsTo(w.ea)
                        for ts_xref in ts_xrefs:
                            a = idc.get_func_attr(ts_xref.frm, 0)
                            b = idc.get_func_attr(xref.frm, 0)
                            if a == b:
                                #print('[debug] ts_xref: {}'.format(idc.get_func_attr(ts_xref.frm, 0)))
                                #print('[debug] xref: {}'.format(idc.get_func_attr(xref.frm, 0)))
                                funcAdds = [idc.get_func_attr(ts_xref.frm, 0)]
            # show only unique func addresses
            funcAdds = list(set(funcAdds))
            if len(funcAdds) == 0:
                print('[!] Function for {} not found. Try manually.'.format(str(s)))
            elif len(funcAdds) > 1:
                print('[!] Multiple functions for {} found. Try manually.'.format(str(s)))
            else:
                print('[*] "{}" in function starting at 0x{:x}'.format(str(s), funcAdds[0]))
                output.append('{}: memAddress(membase, idabase, "0x{:x}"),'.format(str(function), funcAdds[0]))
            match = False

if output:
    print('\n[*] Ida base: {}'.format(hex(idaapi.get_imagebase())))
    print('\n[*] Generating addressTable items:')
    for l in output:
        print(l)
